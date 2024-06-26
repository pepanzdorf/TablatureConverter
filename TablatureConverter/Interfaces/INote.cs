using TablatureConverter.Classes;

namespace TablatureConverter.Interfaces;

public interface INote
{
    void Transpose(int semitones);
    int GetSemitones(); // Will return the number of semitones from the root note C0 = 16.352 Hz = 0 semitones
    int GetOctave(); // Will return the octave of the note
    NoteName GetNote(); // Will return the note name (C, C#, D, D#, E, F, F#, G, G#, A, A#, B)
    double GetFrequency(); // Will return the frequency of the note in Hz
}