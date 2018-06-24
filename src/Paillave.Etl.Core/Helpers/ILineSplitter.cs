using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.Helpers
{
    public interface ILineSplitter
    {
        string[] Split(string line);
    }
}
