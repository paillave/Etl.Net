using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.System.TraceContents
{
    public interface ITraceContent
    {
        TraceLevel Level { get; }
    }
}
