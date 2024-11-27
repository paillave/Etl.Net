using System.Diagnostics;

namespace Paillave.Etl.Core
{
    public class CounterSummaryStreamTraceContent(int counter) : StreamTraceContentBase
    {
        public override TraceLevel Level => TraceLevel.Info;

        public int Counter { get; } = counter;

        public override string Message => $"{this.Counter} row(s) issued";
    }
}
