using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public class SortWarningStreamTraceContent : StreamTraceContentBase
    {
        public SortWarningStreamTraceContent() : base()
        {
        }
        public override TraceLevel Level => TraceLevel.Warning;

        public override string Message => $"lots of rows are sorted and therefore set in memory";
    }
}
