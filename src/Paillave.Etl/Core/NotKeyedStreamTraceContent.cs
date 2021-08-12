using System.Diagnostics;

namespace Paillave.Etl.Core
{
    public class NotKeyedStreamTraceContent : StreamTraceContentBase
    {
        public NotKeyedStreamTraceContent(int lineNumber)
        {
            this.LineNumber = lineNumber;
        }
        public int LineNumber { get; }
        public override TraceLevel Level => TraceLevel.Error;
        public override string Message => $"The stream is not keyed at line {LineNumber} whereas it should be";
    }
}
