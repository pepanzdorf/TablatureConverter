using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TablatureConverter.Interfaces;

namespace TablatureConverter.Classes;

public class TabParser
{
    private readonly TextReader _tabSource;
    private readonly IInstrumentTabParser _instrumentTabParser;
    private readonly IInstrumentTabBuilder _instrumentTabBuilder;
    private readonly TextWriter _tabOutput;
    private readonly int _transposeSemitones;
    private StringBuilder _buffer = new StringBuilder();
    
    public TabParser(TextReader tabSource, TextWriter tabOutput,IInstrumentTabParser instrumentTabParser, IInstrumentTabBuilder instrumentTabBuilder, int transposeSemitones)
    {
        _tabSource = tabSource;
        _tabOutput = tabOutput;
        _instrumentTabParser = instrumentTabParser;
        _instrumentTabBuilder = instrumentTabBuilder;
        _transposeSemitones = transposeSemitones;
    }
    
    public void Parse()
    {
        while (FindInstrumentTab())
        {
            // We found a tabulature, now we need to parse it
            // First we need to separate the tabulature from any extra stuff that might be after it
            (string separatedTabulature, string extra) = SeparateTabulature(_buffer.ToString());
            // Then we parse the tabulature
            (List<MusicalPart> parsedTabulature, Note lowestNote) = _instrumentTabParser.Parse(separatedTabulature);
            // Then we transpose it
            Transpose(parsedTabulature);
            // Then we build it and write it to output
            _tabOutput.Write(_instrumentTabBuilder.Build(parsedTabulature, lowestNote));
            // Then we write the extra stuff that was after the tabulature
            _tabOutput.WriteLine(extra);
            _buffer.Clear();
        }
    }
    
    private bool FindInstrumentTab()
    {
        /*
         * Goes through the input file line by line and finds the next instrument tab.
         * Parts of file that are not tabulature are written to output immediately.
         */
        string? line;
        while ((line = _tabSource.ReadLine()) != null)
        {
            _buffer.AppendLine(line);
            if (_instrumentTabParser.IsPartOfInstrumentTab(line))
            {
                if (_instrumentTabParser.TabIsComplete())
                {
                    return true;
                }
            }
            else
            {
                _tabOutput.Write(_buffer.ToString());
                _buffer.Clear();
            }
        }
        return false;
    }
    
    private (string separatedTabulature, string extra) SeparateTabulature(string tabulature)
    {
        /*
         * Sometimes people put extra stuff after the tabulature, like repetitions or explanations.
         * We need to separate that from the tabulature so that it can be added back later.
         */
        StringBuilder separatedTabulature = new StringBuilder();
        StringBuilder extra = new StringBuilder();
        string[] lines = tabulature.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int shortestLine = lines.OrderBy(s => s.Length).First().Length;
        foreach (string line in lines)
        {
            if (line.Length > shortestLine)
            {
                separatedTabulature.AppendLine(line[..shortestLine]);
                extra.Append(line[shortestLine..] + " ");
            }
            else
            {
                separatedTabulature.AppendLine(line);
            }
        }
        return (separatedTabulature.ToString(), extra.ToString());
    }
    
    private void Transpose(List<MusicalPart> tabulature)
    {
        // Transpose all the tabs in file
        foreach (MusicalPart part in tabulature)
        {
            foreach (IMusicalSymbol symbol in part.Symbols)
            {
                if (symbol is INote note)
                {
                    note.Transpose(_transposeSemitones);
                }
            }
        }
    }
}