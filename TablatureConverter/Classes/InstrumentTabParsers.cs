using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TablatureConverter.Interfaces;

namespace TablatureConverter.Classes;


public class GenericStringInstrumentTabParser : IInstrumentTabParser
{
    protected string[] Strings { get; }
    protected int[] StringOffsets { get; }
    protected int CurrentString { get; set; } = 0;
    
    public GenericStringInstrumentTabParser(string[] strings, int[] stringOffsets)
    {
        Strings = strings;
        StringOffsets = stringOffsets;
    }
    
    public bool IsPartOfInstrumentTab(string line)
    {
        if (line.Length < 2)
        {
            return false;
        }

        if (line[..2] == Strings[CurrentString] + "|")
        {
            ++CurrentString;
            return true;
        }
        else
        {
            CurrentString = 0;
            return false;
        }
    }
    
    public bool TabIsComplete()
    {
        if (CurrentString == Strings.Length)
        {
            CurrentString = 0;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public virtual (List<MusicalPart>, Note lowestNote) Parse(string tablature)
    {
        List<MusicalPart> parsedTablature = new List<MusicalPart>();
        string[] stringTabs = tablature.Split('\n');
        Note lowestNote = Note.FromSemitones(int.MaxValue);
        // Greedily matches anything that is not a dash
        Regex musicalPartRegex = new Regex(@"[^-]+");
        for (int i = 0; i < Strings.Length; ++i)
        {
            // Match all musical parts on the current string
            MatchCollection matched = musicalPartRegex.Matches(stringTabs[i][2..]);
            foreach (Match match in matched)
            {
                // Parse the musical part and get the lowest note in it
                MusicalPart musicalPart = MusicalPartParser.Parse(match.Value, StringOffsets[i], match.Index);
                if (musicalPart.LowestNote.GetSemitones() < lowestNote.GetSemitones())
                {
                    // Update the lowest note in the tablature
                    lowestNote = musicalPart.LowestNote;
                }
                parsedTablature.Add(musicalPart);
            }
        }
        return (parsedTablature, lowestNote);
    }
}


public class GuitarTabParser : GenericStringInstrumentTabParser
{
    public GuitarTabParser() : base(["e", "B", "G", "D", "A", "E"], [52, 47, 43, 38, 33, 28])
    {
    }

    public GuitarTabParser(string[] strings, int[] stringOffsets) : base(strings, stringOffsets)
    {
    }
}

public class BassTabParser : GenericStringInstrumentTabParser
{
    public BassTabParser() : base(["G", "D", "A", "E"], [31, 26, 21, 16])
    {
    }
    
    public BassTabParser(string[] strings, int[] stringOffsets) : base(strings, stringOffsets)
    {
    }
}