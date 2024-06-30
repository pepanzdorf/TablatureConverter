using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TablatureConverter.Interfaces;

namespace TablatureConverter.Classes;


public class GenericStringInstrumentTabBuilder : IInstrumentTabBuilder
{
    protected string[] Strings { get; }
    protected int[] StringOffsets { get; }
    protected int Transpose {get; set; }

    public GenericStringInstrumentTabBuilder(string[] stringNames, int[] stringOffsets)
    {
        Strings = stringNames;
        StringOffsets = stringOffsets;
    }

    public virtual string Build(List<MusicalPart> musicalParts, Note lowestNote)
    {
        Transpose = CalculateTranspose(lowestNote);
        
        var groupsByStart = MusicalPartParser.GroupByStart(musicalParts);
        
        StringBuilder[] strings = new StringBuilder[Strings.Length];
        for (int i = 0; i < strings.Length; ++i)
        {
            strings[i] = new StringBuilder($"{Strings[i]}|-");
        }

        foreach (var group in groupsByStart)
        {
            bool[] availableStrings = new bool[Strings.Length];
            for (int i = 0; i < availableStrings.Length; ++i)
            {
                availableStrings[i] = true;
            }
            bool isSeparator = false;
            foreach (var musicalPart in group)
            {
                if (musicalPart.Symbols.Count == 1 && musicalPart.Symbols[0] is Technique singleTechnique)
                {
                    // Special case for separator
                    if (singleTechnique.Name == "|")
                    {
                        isSeparator = true;
                        continue;
                    }
                }

                int stringIndex = -1;
                if (musicalPart.LowestNote.GetSemitones() != Int32.MaxValue)
                {
                    // Find the string that the lowest note in the part can be played on
                    stringIndex = NoteFinder.Find(Note.FromSemitones(musicalPart.LowestNote.GetSemitones() + Transpose*Constants.TonesInOctave),
                        StringOffsets, availableStrings).instrumentString;
                }
                else
                {
                    // If the part is only techniques, find the first available string
                    for (int i = 0; i < availableStrings.Length; ++i)
                    {
                        if (availableStrings[i])
                        {
                            stringIndex = i;
                            break;
                        }
                    }
                }

                if (stringIndex != -1)
                {
                    // Put the whole part on the string
                    availableStrings[stringIndex] = false;
                    foreach (var symbol in musicalPart.Symbols)
                    {
                        if (symbol is Note note)
                        {
                            note.Transpose(Transpose*Constants.TonesInOctave);
                            int fret = NoteFinder.Find(note, StringOffsets, availableStrings, stringIndex).fret;
                            strings[stringIndex].Append($"{fret}");
                        }
                        else if (symbol is Technique technique)
                        {
                            strings[stringIndex].Append($"{technique.Name}");
                        }
                    }
                }
            }
            
            if (isSeparator)
            {
                for (int i = 0; i < strings.Length; ++i)
                {
                    strings[i].Append('|');
                }
            } else
            {
                // Fill the rest of the strings with dashes
                int longestPart = strings.Max(s => s.Length);
                for (int i = 0; i < strings.Length; ++i)
                {
                    strings[i].Append('-', (longestPart - strings[i].Length) + 1);
                }
            }
        }
        
        return BuildTabFromStrings(strings);
    }
    
    protected int CalculateTranspose(Note lowestNote)
    {
        // Calculate the number of octaves needed to transpose so the tab can be played on the instrument
        int transpose = 0;
        int lowestNoteSemitones = lowestNote.GetSemitones();
        while (lowestNoteSemitones < StringOffsets.Min())
        {
            lowestNoteSemitones += Constants.TonesInOctave;
            ++transpose;
        }
        return transpose;
    }
    
    protected string BuildTabFromStrings(StringBuilder[] strings)
    {
        StringBuilder tab = new StringBuilder();
        tab.AppendLine($"Transposed by: {Transpose} octaves");
        for (int i = 0; i < strings.Length; ++i)
        {
            if (i == strings.Length - 1)
            {
                tab.Append(strings[i].ToString());
            }
            else
            {
                tab.AppendLine(strings[i].ToString());
            }
        }
        return tab.ToString();
    }
}


public class BanjoTabBuilder : GenericStringInstrumentTabBuilder
{
    public BanjoTabBuilder(string[] stringNames, int[] stringOffsets) : base(stringNames, stringOffsets)
    {
    }
    
    public BanjoTabBuilder() : base(["d", "B", "G", "D", "g"], [50, 47, 43, 38, 55])
    {
    }
    
    public override string Build(List<MusicalPart> musicalParts, Note lowestNote)
    {
        Transpose = CalculateTranspose(lowestNote);

        var groupsByStart = MusicalPartParser.GroupByStart(musicalParts);
        
        StringBuilder[] strings = new StringBuilder[Strings.Length];
        for (int i = 0; i < strings.Length; ++i)
        {
            strings[i] = new StringBuilder($"{Strings[i]}|-");
        }

        foreach (var group in groupsByStart)
        {
            // G string (55) is special on banjo and can only be used for open G, has to be handled separately
            bool[] availableStrings = new bool[] { true, true, true, true, false };
            bool gIsAvailable = true;
            bool isSeparator = false;
            foreach (var musicalPart in group)
            {
                if (musicalPart.Symbols.Count == 1 && musicalPart.Symbols[0] is Note singleNote)
                {
                    // Special case for open G on banjo
                    if (singleNote.GetSemitones() + Transpose*Constants.TonesInOctave == 55 && gIsAvailable)
                    {
                        strings[4].Append('0');
                        gIsAvailable = false;
                        continue;
                    }
                }
                
                if (musicalPart.Symbols.Count == 1 && musicalPart.Symbols[0] is Technique singleTechnique)
                {
                    // Special case for separator
                    if (singleTechnique.Name == "|")
                    {
                        isSeparator = true;
                        continue;
                    }
                }

                int stringIndex = -1;
                if (musicalPart.LowestNote.GetSemitones() != Int32.MinValue)
                {
                    // Find the string that the lowest note in the part can be played on
                    stringIndex = NoteFinder.Find(Note.FromSemitones(musicalPart.LowestNote.GetSemitones() + Transpose*Constants.TonesInOctave),
                        StringOffsets, availableStrings).instrumentString;
                }

                if (stringIndex != -1)
                {
                    // Put the whole part on the string
                    availableStrings[stringIndex] = false;
                    foreach (var symbol in musicalPart.Symbols)
                    {
                        if (symbol is Note note)
                        {
                            note.Transpose(Transpose*Constants.TonesInOctave);
                            int fret = NoteFinder.Find(note, StringOffsets, availableStrings, stringIndex).fret;
                            strings[stringIndex].Append($"{fret}");
                        }
                        else if (symbol is Technique technique)
                        {
                            strings[stringIndex].Append($"{technique.Name}");
                        }
                    }
                }
            }
            
            if (isSeparator)
            {
                for (int i = 0; i < strings.Length; ++i)
                {
                    strings[i].Append('|');
                }
            } else
            {
                // Fill the rest of the strings with dashes
                var longestPart = strings.OrderByDescending(s => s.Length).First().Length;
                for (int i = 0; i < strings.Length; ++i)
                {
                    strings[i].Append('-', (longestPart - strings[i].Length) + 1);
                }
            }
        }
        
        return BuildTabFromStrings(strings);
    }
}

public class GuitarTabBuilder : GenericStringInstrumentTabBuilder
{
    public GuitarTabBuilder(string[] stringNames, int[] stringOffsets) : base(stringNames, stringOffsets)
    {
    }
    
    public GuitarTabBuilder() : base(["e", "B", "G", "D", "A", "E"], [52, 47, 43, 38, 33, 28])
    {
    }
}

public class BassTabBuilder : GenericStringInstrumentTabBuilder
{
    public BassTabBuilder(string[] stringNames, int[] stringOffsets) : base(stringNames, stringOffsets)
    {
    } 
    
    public BassTabBuilder() : base(["G", "D", "A", "E"], [31, 26, 21, 16])
    {
    }
}

public class NoteNameBuilder : IInstrumentTabBuilder
{
    public string Build(List<MusicalPart> musicalParts, Note lowestNote)
    {
        var groupsByStart = MusicalPartParser.GroupByStart(musicalParts);
        int biggestGroup = groupsByStart.Max(g => g.Count());
        StringBuilder[] rows = new StringBuilder[biggestGroup];
        for (int i = 0; i < rows.Length; ++i)
        {
            rows[i] = new StringBuilder();
        }
        
        foreach (var group in groupsByStart)
        {
            for (var i = 0; i < group.Count(); ++i)
            {
                var part = group.ElementAt(i);
                foreach (var symbol in part.Symbols)
                {
                    if (symbol is Note note)
                    {
                        rows[i].Append($"{Note.NoteNameToString(note.GetNote())}{note.GetOctave()} ");
                    }
                }
            }
            
            int longestRow = rows.Max(r => r.Length);
            foreach (var row in rows)
            {
                row.Append(' ', longestRow - row.Length);
            }
        }

        
        StringBuilder tab = new StringBuilder();
        
        for (var i = 0; i < rows.Length; ++i)
        {
            string rowString = rows[i].ToString();
            
            if (rowString.Trim().Length == 0)
            {
                continue;
            }
            if (i > 0)
            {
                tab.AppendLine();
            }
            tab.Append(rowString);
            tab.Append('|');
        }
        return tab.ToString();
    }
}
