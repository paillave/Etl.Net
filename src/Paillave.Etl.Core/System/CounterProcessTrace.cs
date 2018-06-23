using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class CounterProcessTrace : StreamProcessTraceBase
    {
        public int Counter { get; private set; }
        public CounterProcessTrace(string streamOperatorName, string side, int counter) : base(streamOperatorName, side, TraceLevel.Verbose, $"counter={counter}")
        {
            this.Counter = counter;
        }
    }
}
