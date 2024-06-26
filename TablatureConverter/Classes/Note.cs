using System;
using TablatureConverter.Interfaces;

namespace TablatureConverter.Classes;


static class Constants
{
    public const int TonesInOctave = 12;
    public const int ConcertPitch = 440;
    public const int A4 = 57;
}

public enum NoteName
{
    C,
    CSharp,
    D,
    DSharp,
    E,
    F,
    FSharp,
    G,
    GSharp,
    A,
    ASharp ,
    B,
}

public class Note(NoteName note, int octave) : IMusicalSymbol, INote
{
    public void Transpose(int semitones)
    {
        NoteName newNote = (NoteName)((GetSemitones() + semitones) % Constants.TonesInOctave);
        int newOctave = (GetSemitones() + semitones) / Constants.TonesInOctave;
        note = newNote;
        octave = newOctave;
    }
    
    public int GetSemitones()
    {
        return Constants.TonesInOctave * octave + (int)note;
    }
    
    public int GetOctave()
    {
        return octave;
    }
    
    public NoteName GetNote()
    {
        return note;
    }
    
    public double GetFrequency()
    {
        return Constants.ConcertPitch * Math.Pow(2, (GetSemitones() - Constants.A4) / (double)Constants.TonesInOctave);
    }
    
    public static Note FromSemitones(int semitones)
    {
        return new Note((NoteName)(semitones % Constants.TonesInOctave), semitones / Constants.TonesInOctave);
    }
}