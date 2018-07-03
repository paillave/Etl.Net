using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.SystemOld
{
    public class RowProcessTrace<I> : StreamProcessTraceBase
    {
        public int Counter { get; }
        public I Row { get; }
        public RowProcessTrace(IEnumerable<string> sourceNodeName, string side, int counter, I row) : base(sourceNodeName, side, TraceLevel.Verbose, $"counter={counter}")
        {
            this.Counter = counter;
            this.Row = row;
        }
    }
}
