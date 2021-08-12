using System.Diagnostics;

namespace Paillave.Etl.Core
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
