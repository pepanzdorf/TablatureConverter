using System;

namespace TablatureConverter.Classes;

public class NoteFinder
{
    public static (int instrumentString, int fret) Find(Note note, int[] stringOffsets, bool[] availableStrings, int forcedString = -1)
    {
        /*
         * Returns the string and fret where the note should be played.
         * If the note is not playable it will return instrumentString -1.
         */
        
        if (stringOffsets.Length != availableStrings.Length)
        {
            throw new ArgumentException("stringOffsets and availableStrings must have the same length");
        }
        
        if (forcedString >= 0 && forcedString < availableStrings.Length)
        {
            if (note.GetSemitones() >= stringOffsets[forcedString])
            {
                return (forcedString, note.GetSemitones() - stringOffsets[forcedString]);
            }
            return (-1, -1);
        }
        
        int semitones = note.GetSemitones();
        int bestString = -1;
        int bestFret = Int32.MaxValue;

        for (int i = 0; i < availableStrings.Length; ++i)
        {
            // Best string is the one with the lowest fret (most of the time)
            if (availableStrings[i] && semitones >= stringOffsets[i])
            {
                int fret = semitones - stringOffsets[i];
                if (fret < bestFret)
                {
                    bestString = i;
                    bestFret = fret;
                }
            }
        }
        
        return (bestString, bestFret);
    }
}
