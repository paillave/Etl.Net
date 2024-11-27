using System.Diagnostics;

namespace Paillave.Etl.Core
{
    public class NotSortedStreamTraceContent(int lineNumber) : StreamTraceContentBase
    {
        public int LineNumber { get; } = lineNumber;

        public override TraceLevel Level => TraceLevel.Error;

        public override string Message => $"The stream is not sorted at line {LineNumber} whereas it should be";
    }
}
