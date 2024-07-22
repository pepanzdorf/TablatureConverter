using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
        [Option('b', "builders", Separator = ' ', Required = false, Default = new []{"banjo"}, HelpText = "Instrument the new tab is for.")]
        public IEnumerable<string> InstrumentTabBuilders { get; set; } = ["banjo"];
        
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
                    string extension = "";
                    foreach (string instrument in o.InstrumentTabBuilders)
                    {
                        extension += instrument + ".";
                    }
                    o.OutputFileName = Path.ChangeExtension(o.InputFileName, $".{extension}tab");
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

                
                string[] instrumentTabBuilderStrings = o.InstrumentTabBuilders.ToArray();
                IInstrumentTabBuilder[] instrumentTabBuilders = new IInstrumentTabBuilder[instrumentTabBuilderStrings.Length];
                
                if (instrumentTabBuilderStrings.Length == 0)
                {
                    Console.Error.WriteLine("No output instrument specified.");
                    return;
                }
                
                if (o.OutputTuning != string.Empty)
                {
                    if (instrumentTabBuilderStrings.Length > 1)
                    {
                        Console.Error.WriteLine("Only one instrument can be used if tuning is specified.");
                        return;
                    }
                    
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
                    
                    switch (instrumentTabBuilderStrings[0])
                    {
                        case "notes":
                            instrumentTabBuilders[0] = new NoteNameBuilder();
                            break;
                        case "generic_string":
                            instrumentTabBuilders[0] = new GenericStringInstrumentTabBuilder(stringNames, stringOffsets);
                            break;
                        case "guitar":
                            instrumentTabBuilders[0] = new GuitarTabBuilder(stringNames, stringOffsets);
                            break;
                        case "banjo":
                            instrumentTabBuilders[0] = new BanjoTabBuilder(stringNames, stringOffsets);
                            break;
                        case "bass":
                            instrumentTabBuilders[0] = new BassTabBuilder(stringNames, stringOffsets);
                            break;
                        default:
                            Console.Error.WriteLine($"Invalid instrument {instrumentTabBuilderStrings[0]}.");
                            return;
                    }
                }
                else
                {
                    for (var i = 0; i < instrumentTabBuilderStrings.Length; i++)
                    {
                        switch (instrumentTabBuilderStrings[i])
                        {
                            case "notes":
                                instrumentTabBuilders[i] = new NoteNameBuilder();
                                break;
                            case "generic_string":
                                Console.Error.WriteLine("Tuning has to be specified for generic string instrument.");
                                return;
                            case "guitar":
                                instrumentTabBuilders[i] = new GuitarTabBuilder();
                                break;
                            case "banjo":
                                instrumentTabBuilders[i] = new BanjoTabBuilder();
                                break;
                            case "bass":
                                instrumentTabBuilders[i] = new BassTabBuilder();
                                break;
                            default:
                                Console.Error.WriteLine($"Invalid instrument {instrumentTabBuilderStrings[i]}.");
                                return;
                        }
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
                                instrumentTabBuilders, o.TransposeSemitones);
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
                    Console.WriteLine("Could not open file. (insufficient rights)");
                }

                #endregion
            });
        }
    }
}
