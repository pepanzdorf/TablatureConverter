using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TablatureConverter.Interfaces;

namespace TablatureConverter.Classes;

public class TabConverter
{
    private readonly TextReader _tabSource;
    private readonly IInstrumentTabParser _instrumentTabParser;
    private readonly IInstrumentTabBuilder[] _instrumentTabBuilders;
    private readonly TextWriter _tabOutput;
    private readonly int _transposeSemitones;
    private readonly StringBuilder _buffer = new StringBuilder();
    
    public TabConverter(TextReader tabSource, TextWriter tabOutput,IInstrumentTabParser instrumentTabParser, IInstrumentTabBuilder[] instrumentTabBuilders, int transposeSemitones)
    {
        _tabSource = tabSource;
        _tabOutput = tabOutput;
        _instrumentTabParser = instrumentTabParser;
        _instrumentTabBuilders = instrumentTabBuilders;
        _transposeSemitones = transposeSemitones;
    }
    
    public void Parse()
    {
        while (FindInstrumentTab())
        {
            // We found a tablature, now we need to parse it
            // First we need to separate the tablature from any extra stuff that might be after it
            (string separatedTablature, string extra) = SeparateTablature(_buffer.ToString());
            // Then we parse the tablature
            (List<MusicalPart> parsedTablature, Note lowestNote) = _instrumentTabParser.Parse(separatedTablature);
            // Then we transpose it
            Transpose(parsedTablature);
            // Then we build for each instrument it and write it to output
            for (var i = 0; i < _instrumentTabBuilders.Length; i++)
            {
                IInstrumentTabBuilder builder = _instrumentTabBuilders[i];
                _tabOutput.Write(builder.Build(parsedTablature, lowestNote));
                if (i < _instrumentTabBuilders.Length - 1)
                {
                    _tabOutput.WriteLine("\n");
                }
            }
            // Then we write the extra stuff that was after the tablature (only once)
            _tabOutput.WriteLine(extra);
            _buffer.Clear();
        }
    }
    
    private bool FindInstrumentTab()
    {
        /*
         * Goes through the input file line by line and finds the next instrument tab.
         * Parts of file that are not tablature are written to output immediately.
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
    
    private (string separatedTablature, string extra) SeparateTablature(string tablature)
    {
        /*
         * Sometimes people put extra stuff after the tablature, like repetitions or explanations.
         * We need to separate that from the tablature so that it can be added back later.
         */
        StringBuilder separatedTablature = new StringBuilder();
        StringBuilder extra = new StringBuilder();
        string[] lines = tablature.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        int shortestLine = lines.Min(s => s.Length);
        foreach (string line in lines)
        {
            if (line.Length > shortestLine)
            {
                separatedTablature.AppendLine(line[..shortestLine]);
                extra.Append(line[shortestLine..] + " ");
            }
            else
            {
                separatedTablature.AppendLine(line);
            }
        }
        return (separatedTablature.ToString(), extra.ToString());
    }
    
    private void Transpose(List<MusicalPart> tablature)
    {
        // Transpose all notes in tab
        foreach (MusicalPart part in tablature)
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