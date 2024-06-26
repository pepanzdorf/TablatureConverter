using System.Collections.Generic;
using TabulatureConverter.Interfaces;

namespace TabulatureConverter.Classes;

public struct MusicalPart
{
    public int Start { get; set; }
    public Note LowestNote { get; set; }
    public List<IMusicalSymbol> Symbols { get; set; }
}