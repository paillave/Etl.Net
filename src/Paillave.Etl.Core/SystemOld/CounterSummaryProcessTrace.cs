using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.SystemOld
{
    public class CounterSummaryProcessTrace : StreamProcessTraceBase
    {
        public int Counter { get; private set; }
        public CounterSummaryProcessTrace(IEnumerable<string> sourceNodeName, string side, int counter) : base(sourceNodeName, side, TraceLevel.Info, $"{counter} row(s) issued")
        {
            this.Counter = counter;
        }
    }
}
