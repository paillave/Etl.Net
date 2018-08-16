using Paillave.Etl.Core.TraceContents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core
{
    public interface ITracer
    {
        void Trace(ITraceContent content);
    }
}
