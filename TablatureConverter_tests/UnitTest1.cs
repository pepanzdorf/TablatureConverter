namespace TablatureConverter_tests;

public class NoteTests
{
    [Fact]
    public void NoteToOffset()
    {
        Note note = new Note(NoteName.C, 4);
        Assert.Equal(48, note.GetSemitones());
        note = new Note(NoteName.C, 0);
        Assert.Equal(0, note.GetSemitones());
        note = new Note(NoteName.ASharp, 3);
        Assert.Equal(46, note.GetSemitones());
        note = new Note(NoteName.DSharp, 2);
        Assert.Equal(27, note.GetSemitones());
    }

    [Fact]
    public void OffsetToNote()
    {
        Note note = Note.FromSemitones(53);
        Assert.Equal(NoteName.F, note.GetNote());
        Assert.Equal(4, note.GetOctave());
        note = Note.FromSemitones(0);
        Assert.Equal(NoteName.C, note.GetNote());
        Assert.Equal(0, note.GetOctave());
        note = Note.FromSemitones(16);
        Assert.Equal(NoteName.E, note.GetNote());
        Assert.Equal(1, note.GetOctave());
        note = Note.FromSemitones(47);
        Assert.Equal(NoteName.B, note.GetNote());
        Assert.Equal(3, note.GetOctave());
    }

    [Fact]
    public void Transposing()
    {
        Note note = new Note (NoteName.A, 4);
        note.Transpose(1);
        Assert.Equal(58, note.GetSemitones());
        note.Transpose(12);
        Assert.Equal(70, note.GetSemitones());
        note.Transpose(-1);
        Assert.Equal(69, note.GetSemitones());
        note.Transpose(0);
        Assert.Equal(69, note.GetSemitones());
        note.Transpose(-6);
        Assert.Equal(63, note.GetSemitones());
    }
}

public class MusicalPartParserTests
{
    [Fact]
    public void ParseOneNote()
    {
        MusicalPart musicalPart = MusicalPartParser.Parse("3", 7, 0);
        Assert.Equal(0, musicalPart.Start);
        Assert.Single(musicalPart.Symbols, item => item is Note);
        Assert.Equal(10, ((Note)musicalPart.Symbols[0]).GetSemitones());
        Assert.Equal(10, musicalPart.LowestNote.GetSemitones());
    }
    
    [Fact]
    public void ParseOneTechnique()
    {
        MusicalPart musicalPart = MusicalPartParser.Parse("h", 7, 15);
        Assert.Equal(15, musicalPart.Start);
        Assert.Single(musicalPart.Symbols, item => item is Technique);
        Assert.Equal("h", ((Technique)musicalPart.Symbols[0]).Name);
        Assert.Equal(int.MaxValue, musicalPart.LowestNote.GetSemitones());
    }

    [Fact]
    public void ParseMultipleSymbols()
    {
        MusicalPart musicalPart = MusicalPartParser.Parse("9h11p9", 54, 24);
        Assert.Equal(24, musicalPart.Start);
        Assert.Equal(5, musicalPart.Symbols.Count);
        Assert.Equal(63, ((Note)musicalPart.Symbols[0]).GetSemitones());
        Assert.Equal("h", ((Technique)musicalPart.Symbols[1]).Name);
        Assert.Equal(65, ((Note)musicalPart.Symbols[2]).GetSemitones());
        Assert.Equal("p", ((Technique)musicalPart.Symbols[3]).Name);
        Assert.Equal(63, ((Note)musicalPart.Symbols[4]).GetSemitones());
        Assert.Equal(63, musicalPart.LowestNote.GetSemitones());
    }
}

public class NoteFinderTests
{
    private readonly int[] _stringOffsets = new int[] { 50, 47, 43, 38, 55 };
    // G string (55) is special on banjo and can only be used for open G, has to be handled separately in the builder
    private readonly bool[] _availableStrings = new bool[] { true, true, true, true, false };

    [Fact]
    public void FindE2OnBanjo()
    {
        (int stringIndex, int fret) = NoteFinder.Find(Note.FromSemitones(28), _stringOffsets, _availableStrings);
        Assert.Equal(-1, stringIndex);
    }
    
    [Fact]
    public void FindE3OnBanjo()
    {
        (int stringIndex, int fret) = NoteFinder.Find(Note.FromSemitones(40), _stringOffsets, _availableStrings);
        Assert.Equal(3, stringIndex);
        Assert.Equal(2, fret);
    }
    
    [Fact]
    public void FindB3OnBanjo()
    {
        (int stringIndex, int fret) = NoteFinder.Find(Note.FromSemitones(47), _stringOffsets, _availableStrings);
        Assert.Equal(1, stringIndex);
        Assert.Equal(0, fret);
    }
    
    [Fact]
    public void FindD4OnBanjoForbiddenOpen()
    {
        bool[] availableStrings = new bool[] { false, true, true, true, false };
        (int stringIndex, int fret) = NoteFinder.Find(Note.FromSemitones(50), _stringOffsets, availableStrings);
        Assert.Equal(1, stringIndex);
        Assert.Equal(3, fret);
    }
    
    [Fact]
    public void FindB3OnBanjoForcedG()
    {
        (int stringIndex, int fret) = NoteFinder.Find(Note.FromSemitones(47), _stringOffsets, _availableStrings, 2);
        Assert.Equal(2, stringIndex);
        Assert.Equal(4, fret);
    }
}

public class GuitarTabParserTests
{
    [Fact]
    public void FindInstrumentTab()
    {
        GuitarTabParser parser = new GuitarTabParser();
        Assert.True(parser.IsPartOfInstrumentTab("e|"));
        Assert.True(parser.IsPartOfInstrumentTab("B|"));
        Assert.True(parser.IsPartOfInstrumentTab("G|"));
        Assert.False(parser.TabIsComplete());
        Assert.True(parser.IsPartOfInstrumentTab("D|"));
        Assert.True(parser.IsPartOfInstrumentTab("A|"));
        Assert.True(parser.IsPartOfInstrumentTab("E|"));
        Assert.True(parser.TabIsComplete());
        Assert.False(parser.IsPartOfInstrumentTab("e"));
        Assert.False(parser.IsPartOfInstrumentTab("F|---"));
        Assert.True(parser.IsPartOfInstrumentTab("e|---"));
        Assert.False(parser.TabIsComplete());
        Assert.False(parser.IsPartOfInstrumentTab("e|---|"));
        Assert.True(parser.IsPartOfInstrumentTab("e|---|\n"));
    }
}