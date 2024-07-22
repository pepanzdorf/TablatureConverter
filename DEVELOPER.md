# Developer Documentation

Higher-level overview of the project.

## Architecture

The `Program` class `Main` method is the entry point of the application. It parses the command line arguments
opens the input file and passes relevant to the `TabConverter` class.

### TabConverter

The main class `TabConverter` instantiated with the following arguments:
- `TextReader` - input text reader
- `TextWriter` - output text writer
- `IInstrumentTabParser` - instrument tab parser - class implementing `IInstrumentTabParser` interface
- `IInstrumentTabBuilder[]` - instrument tab builders - array of classes implementing `IInstrumentTabBuilder` interface
- `int` - transposition by semitones

The `TabConverter` class reads the input file line by line and checks if
the line contains a tab using `IsPartOfInstrumentTab` method of `IInstrumentTabParser` interface.
The lines are read until the whole tab is found by checking `TabIsComplete` method of `IInstrumentTabParser` interface.

When the tab is found, the `TabConverter` class calls `Parse` method of `IInstrumentTabParser` interface.
The `Parse` method returns the parsed tab as a list of `MusicalPart` structs and also the
lowest note of the tab.

The `TabConverter` class then transposes the parsed tab by the number of semitones specified in the constructor.

The `Build` method of each `IInstrumentTabBuilder` interface is called with the transposed tab and the lowest note.
Each builder builds the tab and returns the result as a string. The results are written to the output file.

### IInstrumentTabParser

The `IInstrumentTabParser` interface defines the following methods:
- `bool IsPartOfInstrumentTab(string line)` - checks if the line contains a tab
- `bool TabIsComplete()` - checks if the tab is complete
- `(List<MusicalPart>, Note lowestNote) Parse(string tablature)` - parses the tab and returns the list of `MusicalPart` structs and the lowest note

### IInstrumentTabBuilder

The `IInstrumentTabBuilder` interface defines the following method:
- `string Build(List<MusicalPart> musicalParts, Note lowestNote)` - builds the tab and returns the result as a string

### MusicalPart

The `MusicalPart` struct represents a part of the tab and contains the following fields:
- `public int Start { get; set; }` - start position of the part
- `public Note LowestNote { get; set; }` - lowest note of the part
- `public List<IMusicalSymbol> Symbols { get; set; }` - list of musical symbols (notes and techniques) of the part

#### IMusicalSymbol

The `IMusicalSymbol` interface defines no methods and is used as a base interface for `Note` and `Technique` classes.

##### Note

The `Note` class represents a note and contains the following fields:
- `NoteName note` - NoteName enum value
- `int octave` - octave number

and methods:
- `public void Transpose(int semitones)` - transposes the note by the specified number of semitones
- `public int GetSemitones()` - returns the number of semitones from C0
- `public int GetOctave()` - returns the octave number
- `public NoteName GetNote()` - returns the note name (from NoteName enum)
- `public double GetFrequency()` - returns the frequency of the note
- `public static Note FromSemitones(int semitones)` - creates a note from the number of semitones from C0
- `public static string NoteNameToString(NoteName note)` - converts the NoteName from enum to a string


##### Technique

The `Technique` class represents a technique and contains
only one field:
- `string Name` - technique name

and no methods.

## Creating a new instrument tab parser

To create a new instrument tab parser, create a new class that implements the `IInstrumentTabParser` interface.
The class should implement all the methods of the interface.

To parse the tab of string instruments, the `MusicalPartParser` class can be used. Its
`Parse` method accepts:
- `string musicalString` - the part to parse
- `int baseOffset` - if part `3h5` is parsed, the `baseOffset` is the semitones of the string the part is found on.
- `int foundAt` - the position of the part in the string

and returns the parsed part as a `MusicalPart` struct.

## Creating a new instrument tab builder

To create a new instrument tab builder, create a new class that implements the `IInstrumentTabBuilder` interface.
The class should implement all the methods of the interface.

To build the tab for string instruments, the `NoteFinder` class can be used. Its
`Find` method accepts:
- `Note note` - the note to be found
- `int[] stringOffsets` - the semitones of the strings from C0
- `bool[] availableStrings` - the availability of the strings
- optional `int forcedString` - index of the string to force the note to be played on

and returns the string number and the fret number of the note as `tuple` of `int`.