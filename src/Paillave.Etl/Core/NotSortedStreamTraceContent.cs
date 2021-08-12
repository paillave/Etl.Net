using System.Diagnostics;

namespace Paillave.Etl.Core
{
    public class NotSortedStreamTraceContent : StreamTraceContentBase
    {
        public NotSortedStreamTraceContent(int lineNumber)
        {
            this.LineNumber = lineNumber;
        }

        public int LineNumber { get; }

        public override TraceLevel Level => TraceLevel.Error;

        public override string Message => $"The stream is not sorted at line {LineNumber} whereas it should be";
    }
}
