using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TabulatureConverter.Interfaces;

namespace TabulatureConverter.Classes;

public static class MusicalPartParser
{
    public static MusicalPart Parse(string musicalString, int baseOffset, int foundAt)
    {
        // Match either a number (fret/note) or a sequence of non-numbers (technique)
        Regex musicalPartRegex = new Regex(@"\d+|[^\d]+");
        MusicalPart musicalPart = new MusicalPart();
        musicalPart.Start = foundAt;
        Note lowestNote = Note.FromSemitones(Int32.MaxValue);
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
}