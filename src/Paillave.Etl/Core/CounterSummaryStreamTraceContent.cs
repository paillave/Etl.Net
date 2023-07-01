using System.Diagnostics;

namespace Paillave.Etl.Core
{
    public class CounterSummaryStreamTraceContent : StreamTraceContentBase
    {
        public CounterSummaryStreamTraceContent(int counter)
        {
            this.Counter = counter;
        }
        public override TraceLevel Level => TraceLevel.Info;

        public int Counter { get; }

        public override string Message => $"{this.Counter} row(s) issued";
    }
}
