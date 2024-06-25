using System;
using System.Diagnostics;
using System.IO;
using TabulatureConverter.Classes;
using TabulatureConverter.Interfaces;
using CommandLine;

namespace TabulatureConverter
{
    
    public class Options
    {
        [Option('i', "input", Required = true, Default = "", HelpText = "Input file-name.")]
        public string InputFileName { get; set; } = string.Empty;
        
        [Option('o', "output", Required = false, Default = "", HelpText = "Output file-name.")]
        public string OutputFileName { get; set; } = string.Empty;
        
        [Option('t', "transpose", Required = false, Default = 0, HelpText = "Transpose by this many semitones.")]
        public int TransposeSemitones { get; set; } = 0;
        
        [Option('p', "parser", Required = false, Default = "guitar", HelpText = "Instrument the original tab is for.")]
        public string InstrumentTabParser { get; set; } = "guitar";
        
        [Option('b', "builder", Required = false, Default = "banjo", HelpText = "Instrument the new tab is for.")]
        public string InstrumentTabBuilder { get; set; } = "banjo";
        
        [Option("input-string-names", Required = false, Default = "", HelpText = "Names of the strings of the original instrument. (alternative tuning)")]
        public string InputStringNames { get; set; } = string.Empty;
        
        [Option("input-string-offsets", Required = false, Default = "", HelpText = "Offsets of the strings of the original instrument. (alternative tuning")]
        public string InputStringOffsets { get; set; } = string.Empty;
        
        [Option("output-string-names", Required = false, Default = "", HelpText = "Names of the strings of the new instrument. (alternative tuning)")]
        public string OutputStringNames { get; set; } = string.Empty;
        
        [Option("output-string-offsets", Required = false, Default = "", HelpText = "Offsets of the strings of the new instrument. (alternative tuning)")]
        public string OutputStringOffsets { get; set; } = string.Empty;
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                #region Argument parsing
                
                if (o.OutputFileName == string.Empty)
                {
                    o.OutputFileName = Path.ChangeExtension(o.InputFileName, ".banjo.txt");
                }
                
                IInstrumentTabParser instrumentTabParser = new GuitarTabParser();
                if (o.InputStringNames != string.Empty && o.InputStringOffsets != string.Empty)
                {
                    string[] stringNames = o.InputStringNames.Split(';');
                    string[] stringOffsetsAsStrings = o.InputStringOffsets.Split(';');
                    if (stringNames.Length != stringOffsetsAsStrings.Length)
                    {
                        Console.Error.WriteLine("Invalid number of string names and string offsets.");
                        return;
                    }

                    int[] stringOffsets;
                    try
                    {
                        stringOffsets = Array.ConvertAll(stringOffsetsAsStrings, int.Parse);
                    }
                    catch (FormatException)
                    {
                        Console.Error.WriteLine("Invalid string offsets (tuning)");
                        return;
                    }
                    switch (o.InstrumentTabParser)
                    {
                        case "":
                            instrumentTabParser = new GuitarTabParser(stringNames, stringOffsets);
                            break;
                        case "guitar":
                            instrumentTabParser = new GuitarTabParser(stringNames, stringOffsets);
                            break;
                        case "bass":
                            // instrumentTabParse = new BassTabParser(stringNames, stringOffsets);
                            break;
                        default:
                            Console.Error.WriteLine($"Invalid instrument {o.InstrumentTabParser}.");
                            return;
                    }
                }
                else if (o.InstrumentTabParser == "")
                {
                    instrumentTabParser = new GuitarTabParser();
                }
                else if (o.InstrumentTabParser == "guitar")
                {
                    instrumentTabParser = new GuitarTabParser();
                }
                else if (o.InstrumentTabParser == "bass")
                {
                    // instrumentTabParser = new BassTabParser();
                }
                else
                {
                    Console.Error.WriteLine($"Invalid instrument {o.InstrumentTabParser}.");
                    return;
                }
                
                IInstrumentTabBuilder instrumentTabBuilder = new BanjoTabBuilder();
                
                if (o.OutputStringNames != string.Empty && o.OutputStringOffsets != string.Empty)
                {
                    string[] stringNames = o.OutputStringNames.Split(';');
                    string[] stringOffsetsAsStrings = o.OutputStringOffsets.Split(';');
                    if (stringNames.Length != stringOffsetsAsStrings.Length)
                    {
                        Console.Error.WriteLine("Invalid number of string names and string offsets.");
                        return;
                    }

                    int[] stringOffsets;
                    try
                    {
                        stringOffsets = Array.ConvertAll(stringOffsetsAsStrings, int.Parse);
                    }
                    catch (FormatException)
                    {
                        Console.Error.WriteLine("Invalid string offsets (tuning)");
                        return;
                    }
                    switch (o.InstrumentTabBuilder)
                    {
                        case "":
                            instrumentTabBuilder = new BanjoTabBuilder(stringNames, stringOffsets);
                            break;
                        case "banjo":
                            instrumentTabBuilder = new BanjoTabBuilder(stringNames, stringOffsets);
                            break;
                        case "bass":
                            // instrumentTabBuilder = new BassTabParser(stringNames, stringOffsets);
                            break;
                        default:
                            Console.Error.WriteLine($"Invalid instrument {o.InstrumentTabParser}.");
                            return;
                    }
                }
                else if (o.InstrumentTabBuilder == "")
                {
                    instrumentTabBuilder = new BanjoTabBuilder();
                }
                else if (o.InstrumentTabBuilder == "banjo")
                {
                    instrumentTabBuilder = new BanjoTabBuilder();
                }
                else if (o.InstrumentTabParser == "bass")
                {
                    instrumentTabBuilder = new BassTabBuilder();
                }
                else
                {
                    Console.Error.WriteLine($"Invalid instrument {o.InstrumentTabParser}.");
                    return;
                }

                #endregion

                #region Tab parsing

                /*
                 * Parses the tabulature from the input file, transposes it and writes the result to the output file.
                 * The tabulature is parsed by the corresponding IInstrumentTabParser.
                 * Then it is rewritten into tabulature for a different instrument by IInstrumentTabBuilder.
                 */

                try
                {
                    using (StreamReader tabSource = new StreamReader(o.InputFileName))
                    {
                        using (StreamWriter tabOutput = new StreamWriter(o.OutputFileName))
                        {
                            TabParser tabParser = new TabParser(tabSource, tabOutput, instrumentTabParser,
                                instrumentTabBuilder, o.TransposeSemitones);
                            tabParser.Parse();
                        }
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Could not open file.");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Could not open file.");
                }

                #endregion
            });
        }
    }
}
