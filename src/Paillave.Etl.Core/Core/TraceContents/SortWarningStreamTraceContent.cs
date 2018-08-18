using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public class SortWarningStreamTraceContent : StreamTraceContentBase
    {
        public SortWarningStreamTraceContent(string streamName) : base(streamName)
        {
        }
        public override TraceLevel Level => TraceLevel.Warning;

        protected override string GetMessage() => $"lots of rows are sorted and therefore set in memory";
    }
}
