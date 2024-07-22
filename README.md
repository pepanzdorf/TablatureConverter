# Tablature converter

This is a simple tool to convert ASCII tablature from one instrument to another.

## Dependencies

It uses only the CommandLineParser library to parse the command line arguments.

If you also want to run the tests, you will need the XUnit library.


## Usage

The tool is a command line application. You can use it by running the following minimal command in the project directory:

```
dotnet run -- -i <input file>
```

or you can build the project and run the executable.

The following command line arguments are available:

```
-i, --input        Required. Input file-name.
-o, --output       (Default: <input-file-name>.<instruments>.tab) Output file-name.
-t, --transpose    (Default: 0) Transpose by this many semitones.
-p, --parser       (Default: guitar) Instrument the original tab is for.
-b, --builders     (Default: [banjo]) Instruments the new tab is for. Multiple instruments can be specified.
--input-tuning     (Default: ) Name of the tuning file of the chosen input instrument. (alternative tuning)
--output-tuning    (Default: ) Name of the tuning file of the chosen output instrument. (alternative tuning)
--help             Display this help screen.
```

### Examples

To convert a guitar tab 'input.txt' to a banjo tab 'output.txt' transposed two semitones higher:
```
dotnet run -- -i input.txt -o output.txt -t 2 -p guitar -b banjo
```

To convert a guitar tab 'input.txt' to a banjo and bass tab 'input.banjo.bass.tab':
```
dotnet run -- -i input.txt -p guitar -b banjo bass
```

To convert a guitar tab 'input.txt' to a balalaika tab 'balalaika.tab' using tuning file:
```
dotnet run -- -i input.txt -o balalaika.tab -p guitar -b generic_string --output-tuning balalaika.tuning
```

To convert a guitar tab 'input.txt' to a banjo tab 'output.txt' using alternative tuning parser:
```
dotnet run -- -i input.txt -o output.txt -p guitar -b banjo --input-tuning guitar_drop_d.tuning
```

## Supported instruments

Parsing:
- guitar
- bass
- generic_string

Building:
- guitar
- bass
- banjo
- notes (will write the tablature as sequence of notes)
- generic_string

In both cases, the generic_string instrument will use the tuning file to determine the tuning of the instrument,
is not limited to a specific number of strings and parses in a guitar-like manner.

The following are equivalent (if the guitar.tuning file is the standard guitar tuning):

input.txt:
```
G|-------------------|
D|------5------------|
A|-7--7---7--5-3--2--|
E|-------------------|
```


```
dotnet run -- -i input.txt -o output.txt -p bass -b generic_string --output-tuning guitar.tuning
```
```
dotnet run -- -i input.txt -o output.txt -p bass -b guitar
```

output.txt:
```
Transposed by: 1 octaves
e|---------------|
B|---------------|
G|-----0---------|
D|-2-2---2-0-----|
A|-----------3-2-|
E|---------------|
```

### Tuning files

The tuning files are simple text files that contain the tuning of the instrument. The first line are space separated names of the strings from highest to lowest.
The second line are space separated offsets in semitones from the C0 note.
For example, the standard guitar tuning file looks like this:
```
e B G D A E
52 47 43 38 33 28
```

explained as:
- the highest string is tuned to E4 and is 52 semitones higher than the C0 note and is usually written as 'e'
- the second string is tuned to B3 and is 47 semitones higher than the C0 note and is usually written as 'B'
- etc.
