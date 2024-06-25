using System.Collections.Generic;
using TabulatureConverter.Classes;

namespace TabulatureConverter.Interfaces;

public interface IInstrumentTabParser
{
    bool IsPartOfInstrumentTab(string line);
    bool TabIsComplete();
    (List<MusicalPart>, Note lowestNote) Parse(string tabulature);
}