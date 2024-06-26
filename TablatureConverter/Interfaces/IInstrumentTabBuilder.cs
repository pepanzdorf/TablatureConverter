using System.Collections.Generic;
using TabulatureConverter.Classes;

namespace TabulatureConverter.Interfaces;

public interface IInstrumentTabBuilder
{
    string Build(List<MusicalPart> musicalParts, Note lowestNote);
}