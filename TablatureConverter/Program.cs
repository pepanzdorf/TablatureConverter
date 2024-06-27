using System;
using System.IO;
using CommandLine;
using TablatureConverter.Classes;
using TablatureConverter.Interfaces;

namespace TablatureConverter
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
        
        [Option("input-tuning", Required = false, Default = "", HelpText = "Name of the tuning file of the chosen input instrument. (alternative tuning)")]
        public string InputTuning { get; set; } = string.Empty;
        
        [Option("output-tuning", Required = false, Default = "", HelpText = "Name of the tuning file of the chosen output instrument. (alternative tuning)")]
        public string OutputTuning { get; set; } = string.Empty;
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
                    o.OutputFileName = Path.ChangeExtension(o.InputFileName, $".{o.InstrumentTabBuilder}.tab");
                }

                IInstrumentTabParser instrumentTabParser;
                if (o.InputTuning != string.Empty)
                {
                    string[] stringNames;
                    int[] stringOffsets;
                    
                    try
                    {
                        (stringNames, stringOffsets) = TuningReader.Read(o.InputTuning);
                    }
                    catch (IOException)
                    {
                        Console.Error.WriteLine("Could not open tuning file.");
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.Error.WriteLine("Could not open tuning file. (insufficient rights)");
                        return;
                    }
                    catch (FormatException)
                    {
                        Console.Error.WriteLine("Invalid tuning file.");
                        return;
                    }

                    switch (o.InstrumentTabParser)
                    {
                        case "generic_string":
                            instrumentTabParser = new GenericStringInstrumentTabParser(stringNames, stringOffsets);
                            break;
                        case "guitar":
                            instrumentTabParser = new GuitarTabParser(stringNames, stringOffsets);
                            break;
                        case "bass":
                            instrumentTabParser = new BassTabParser(stringNames, stringOffsets);
                            break;
                        default:
                            Console.Error.WriteLine($"Invalid instrument {o.InstrumentTabParser}.");
                            return;
                    }
                }
                else
                {
                    switch (o.InstrumentTabParser)
                    {
                        case "generic_string":
                            Console.Error.WriteLine("Tuning has to be specified for generic string instrument.");
                            return;
                        case "guitar":
                            instrumentTabParser = new GuitarTabParser();
                            break;
                        case "bass":
                            instrumentTabParser = new BassTabParser();
                            break;
                        default:
                            Console.Error.WriteLine($"Invalid instrument {o.InstrumentTabParser}.");
                            return;
                    }   
                }

                
                IInstrumentTabBuilder instrumentTabBuilder;
                
                if (o.OutputTuning != string.Empty)
                {
                    string[] stringNames;
                    int[] stringOffsets;
                    
                    try
                    {
                        (stringNames, stringOffsets) = TuningReader.Read(o.OutputTuning);
                    }
                    catch (IOException)
                    {
                        Console.Error.WriteLine("Could not open tuning file.");
                        return;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Console.Error.WriteLine("Could not open tuning file. (insufficient rights)");
                        return;
                    }
                    catch (FormatException)
                    {
                        Console.Error.WriteLine("Invalid tuning file.");
                        return;
                    }
                    
                    switch (o.InstrumentTabBuilder)
                    {
                        case "generic_string":
                            instrumentTabBuilder = new GenericStringInstrumentTabBuilder(stringNames, stringOffsets);
                            break;
                        case "guitar":
                            instrumentTabBuilder = new GuitarTabBuilder(stringNames, stringOffsets);
                            break;
                        case "banjo":
                            instrumentTabBuilder = new BanjoTabBuilder(stringNames, stringOffsets);
                            break;
                        case "bass":
                            instrumentTabBuilder = new BassTabBuilder(stringNames, stringOffsets);
                            break;
                        default:
                            Console.Error.WriteLine($"Invalid instrument {o.InstrumentTabParser}.");
                            return;
                    }
                }
                else
                {
                    switch (o.InstrumentTabBuilder)
                    {
                        case "generic_string":
                            Console.Error.WriteLine("Tuning has to be specified for generic string instrument.");
                            return;
                        case "guitar":
                            instrumentTabBuilder = new GuitarTabBuilder();
                            break;
                        case "banjo":
                            instrumentTabBuilder = new BanjoTabBuilder();
                            break;
                        case "bass":
                            instrumentTabBuilder = new BassTabBuilder();
                            break;
                        default:
                            Console.Error.WriteLine($"Invalid instrument {o.InstrumentTabParser}.");
                            return;
                    }
                }

                #endregion

                #region Tab parsing

                /*
                 * Parses the tablature from the input file, transposes it and writes the result to the output file.
                 * The tablature is parsed by the corresponding IInstrumentTabParser.
                 * Then it is rewritten into tablature for a different instrument by IInstrumentTabBuilder.
                 */

                try
                {
                    using (StreamReader tabSource = new StreamReader(o.InputFileName))
                    {
                        using (StreamWriter tabOutput = new StreamWriter(o.OutputFileName))
                        {
                            TabConverter tabConverter = new TabConverter(tabSource, tabOutput, instrumentTabParser,
                                instrumentTabBuilder, o.TransposeSemitones);
                            tabConverter.Parse();
                        }
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Could not open file.");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Could not open file. (insufficient rights");
                }

                #endregion
            });
        }
    }
}
