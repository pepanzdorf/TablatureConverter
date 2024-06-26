using System;
using System.IO;

namespace TablatureConverter.Classes;

public class TuningReader
{
    public static (string[], int[]) Read(string fileName)
    {
        // Read the tuning file and return the string names and offsets.
        // Ignores all but the first two lines of the file.
        using (StreamReader tuningFile = new StreamReader(fileName))
        {
            string? line = tuningFile.ReadLine();
            if (line == null)
            {
                throw new FormatException();
            }
            string[] stringNames = line.Split(' ');
            line = tuningFile.ReadLine();
            if (line == null)
            {
                throw new FormatException();
            }
            int[] stringOffsets = Array.ConvertAll(line.Split(' '), int.Parse);
            
            if (stringNames.Length != stringOffsets.Length)
            {
                throw new FormatException();
            }
            
            return (stringNames, stringOffsets);
        }
    }
}