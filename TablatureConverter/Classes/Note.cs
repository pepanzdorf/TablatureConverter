using System;
using TablatureConverter.Interfaces;

namespace TablatureConverter.Classes;


static class Constants
{
    public const int TonesInOctave = 12;
    public const int ConcertPitch = 440; // standard frequency of A4
    public const int A4 = 57; // A4 is the 57th semitone from C0
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
        int newSemitones = GetSemitones() + semitones;
        note = (NoteName)(newSemitones % Constants.TonesInOctave);
        octave = newSemitones / Constants.TonesInOctave;
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
    
    // Create a note from the number of semitones from C0
    public static Note FromSemitones(int semitones)
    {
        return new Note((NoteName)(semitones % Constants.TonesInOctave), semitones / Constants.TonesInOctave);
    }

    public static string NoteNameToString(NoteName note) => note switch
    {
        NoteName.C => "C",
        NoteName.CSharp => "C#",
        NoteName.D => "D",
        NoteName.DSharp => "D#",
        NoteName.E => "E",
        NoteName.F => "F",
        NoteName.FSharp => "F#",
        NoteName.G => "G",
        NoteName.GSharp => "G#",
        NoteName.A => "A",
        NoteName.ASharp => "A#",
        NoteName.B => "B",
        _ => throw new ArgumentOutOfRangeException(nameof(note), note, null)
    };
}