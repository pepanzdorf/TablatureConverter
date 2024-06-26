using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TablatureConverter.Interfaces;

namespace TablatureConverter.Classes;

public class GuitarTabParser : IInstrumentTabParser
{
    private readonly string[] _strings;
    private readonly int[] _stringOffsets;
    private int _currentString = 0;

    public GuitarTabParser()
    {
        _strings = new string[] { "e", "B", "G", "D", "A", "E" };
        _stringOffsets = new int[] { 52, 47, 43, 38, 33, 28 };
    }
    
    public GuitarTabParser(string[] strings, int[] stringOffsets)
    {
        _strings = strings;
        _stringOffsets = stringOffsets;
    }
    
    public bool IsPartOfInstrumentTab(string line)
    {
        if (_currentString == _strings.Length)
        {
            _currentString = 0;
            return false;
        }
        
        if (line.Length < 2)
        {
            return false;
        }

        if (line[..2] == _strings[_currentString] + "|")
        {
            ++_currentString;
            return true;
        }
        else
        {
            _currentString = 0;
            return false;
        }
    }
    
    public bool TabIsComplete()
    {
        if (_currentString == _strings.Length)
        {
            _currentString = 0;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public (List<MusicalPart>, Note lowestNote) Parse(string tabulature)
    {
        List<MusicalPart> parsedTabulature = new List<MusicalPart>();
        string[] stringTabs = tabulature.Split('\n');
        Note lowestNote = Note.FromSemitones(Int32.MaxValue);
        // Greedily matches anything that is not a dash
        Regex musicalPartRegex = new Regex(@"[^-]+");
        for (int i = 0; i < _strings.Length; ++i)
        {
            // Match all musical parts on the current string
            MatchCollection matched = musicalPartRegex.Matches(stringTabs[i][2..]);
            foreach (Match match in matched)
            {
                // Parse the musical part and get the lowest note in it
                MusicalPart musicalPart = MusicalPartParser.Parse(match.Value, _stringOffsets[i], match.Index);
                if (musicalPart.LowestNote.GetSemitones() < lowestNote.GetSemitones())
                {
                    // Update the lowest note in the tabulature
                    lowestNote = musicalPart.LowestNote;
                }
                parsedTabulature.Add(musicalPart);
            }
        }
        return (parsedTabulature, lowestNote);
    }
}

public class BassTabParser : IInstrumentTabParser
{
    private readonly string[] _strings;
    private readonly int[] _stringOffsets;
    private int _currentString = 0;

    public BassTabParser()
    {
        _strings = new string[] { "G", "D", "A", "E" };
        _stringOffsets = new int[] { 31, 26, 21, 16 };
    }
    
    public BassTabParser(string[] strings, int[] stringOffsets)
    {
        _strings = strings;
        _stringOffsets = stringOffsets;
    }
    
    public bool IsPartOfInstrumentTab(string line)
    {
        if (_currentString == _strings.Length)
        {
            _currentString = 0;
            return false;
        }
        
        if (line.Length < 2)
        {
            return false;
        }

        if (line[..2] == _strings[_currentString] + "|")
        {
            ++_currentString;
            return true;
        }
        else
        {
            _currentString = 0;
            return false;
        }
    }
    
    public bool TabIsComplete()
    {
        if (_currentString == _strings.Length)
        {
            _currentString = 0;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public (List<MusicalPart>, Note lowestNote) Parse(string tabulature)
    {
        List<MusicalPart> parsedTabulature = new List<MusicalPart>();
        string[] stringTabs = tabulature.Split('\n');
        Note lowestNote = Note.FromSemitones(Int32.MaxValue);
        // Greedily matches anything that is not a dash
        Regex musicalPartRegex = new Regex(@"[^-]+");
        for (int i = 0; i < _strings.Length; ++i)
        {
            // Match all musical parts on the current string
            MatchCollection matched = musicalPartRegex.Matches(stringTabs[i][2..]);
            foreach (Match match in matched)
            {
                // Parse the musical part and get the lowest note in it
                MusicalPart musicalPart = MusicalPartParser.Parse(match.Value, _stringOffsets[i], match.Index);
                if (musicalPart.LowestNote.GetSemitones() < lowestNote.GetSemitones())
                {
                    // Update the lowest note in the tabulature
                    lowestNote = musicalPart.LowestNote;
                }
                parsedTabulature.Add(musicalPart);
            }
        }
        return (parsedTabulature, lowestNote);
    }
}