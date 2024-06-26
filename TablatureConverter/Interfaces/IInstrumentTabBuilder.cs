using System.Collections.Generic;
using TablatureConverter.Classes;

namespace TablatureConverter.Interfaces;

public interface IInstrumentTabBuilder
{
    string Build(List<MusicalPart> musicalParts, Note lowestNote);
}