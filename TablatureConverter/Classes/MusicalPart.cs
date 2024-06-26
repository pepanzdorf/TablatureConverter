using System.Collections.Generic;
using TablatureConverter.Interfaces;

namespace TablatureConverter.Classes;

public struct MusicalPart
{
    public int Start { get; set; }
    public Note LowestNote { get; set; }
    public List<IMusicalSymbol> Symbols { get; set; }
}