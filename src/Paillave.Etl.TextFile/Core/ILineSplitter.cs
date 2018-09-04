using System.Collections.Generic;

namespace Paillave.Etl.TextFile.Core
{
    public interface ILineSplitter
    {
        IList<string> Split(string line);
        string Join(IEnumerable<string> line);
    }
}