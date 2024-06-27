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
        
        var groupsByStart = GroupByStart(musicalParts);
        
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
    
    protected IGrouping<int, MusicalPart>[] GroupByStart(List<MusicalPart> musicalParts)
    {
        MusicalPart[] sortedMusicalParts = musicalParts.ToArray();
        Array.Sort(sortedMusicalParts, (a, b) =>
        {
            int primaryComparison = a.Start.CompareTo(b.Start);
            if (primaryComparison != 0)
            {
                return primaryComparison;
            }
            // Sort by lowest note if start is the same
            return a.LowestNote.GetSemitones().CompareTo(b.LowestNote.GetSemitones()); 
        });
        
        var groupsByStart =
            from musicalPart in sortedMusicalParts
            group musicalPart by musicalPart.Start;
        
        return groupsByStart.ToArray();
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

        var groupsByStart = GroupByStart(musicalParts);
        
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