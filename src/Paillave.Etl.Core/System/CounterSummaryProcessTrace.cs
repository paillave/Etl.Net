using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class CounterSummaryProcessTrace : StreamProcessTraceBase
    {
        public int Counter { get; private set; }
        public CounterSummaryProcessTrace(string streamOperatorName, string side, int counter) : base(streamOperatorName, side, TraceLevel.Info, $"{counter} row(s) issued")
        {
            this.Counter = counter;
        }
    }
}
