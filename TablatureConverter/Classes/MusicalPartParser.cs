using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TablatureConverter.Interfaces;

namespace TablatureConverter.Classes;

public static class MusicalPartParser
{
    public static MusicalPart Parse(string musicalString, int baseOffset, int foundAt)
    {
        // Match either a number (fret/note) or a sequence of non-numbers (technique)
        Regex musicalPartRegex = new Regex(@"\d+|[^\d]+");
        MusicalPart musicalPart = new MusicalPart();
        musicalPart.Start = foundAt;
        Note lowestNote = Note.FromSemitones(int.MaxValue);
        var symbols = new List<IMusicalSymbol>();
        var matches = musicalPartRegex.Matches(musicalString);
        for (int i = 0; i < matches.Count; ++i)
        {
            // If the match is a number, it's a note, otherwise it's a technique
            if (int.TryParse(matches[i].Value, out int fret))
            {
                Note newNote = Note.FromSemitones(baseOffset + fret);
                if (newNote.GetSemitones() < lowestNote.GetSemitones())
                {
                    // We need to find the lowest note in the part to help find the lowest note in the whole tab
                    // so we know how much to transpose later
                    lowestNote = newNote;
                }
                symbols.Add(newNote);
            }
            else
            {
                symbols.Add(new Technique{Name = matches[i].Value});
            }
        }
        musicalPart.Symbols = symbols;
        musicalPart.LowestNote = lowestNote;
        return musicalPart;
    }
    
    public static IGrouping<int, MusicalPart>[] GroupByStart(List<MusicalPart> musicalParts)
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
}