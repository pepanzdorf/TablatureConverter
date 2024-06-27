using System.Collections.Generic;
using TablatureConverter.Classes;

namespace TablatureConverter.Interfaces;

public interface IInstrumentTabParser
{
    bool IsPartOfInstrumentTab(string line);
    bool TabIsComplete();
    (List<MusicalPart>, Note lowestNote) Parse(string tablature);
}