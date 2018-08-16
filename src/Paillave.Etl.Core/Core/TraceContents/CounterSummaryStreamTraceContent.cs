using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public class CounterSummaryStreamTraceContent : StreamTraceContentBase
    {
        public CounterSummaryStreamTraceContent(string outputName, int counter) : base(outputName)
        {
            this.Counter = counter;
        }
        public override TraceLevel Level => TraceLevel.Info;

        public int Counter { get; }

        protected override string GetMessage() => $"{this.Counter} row(s) issued";
    }
}
