using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class StreamProcessTraceBase : ProcessTrace
    {
        public string Side { get; private set; }
        public StreamProcessTraceBase(string streamOperatorName, string side, TraceLevel level, string message) : base(streamOperatorName, message, level)
        {
            this.Side = side;
        }
        public override string ToString()
        {
            return $"{this.StreamOperatorName}.{this.Side}:[{this.Level}]-{this.Message}";
        }
    }
}
