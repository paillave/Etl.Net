using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public interface ITraceContent
    {
        TraceLevel Level { get; }
    }
}
