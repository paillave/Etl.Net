using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.SystemOld
{
    public abstract class StreamProcessTraceBase : ProcessTrace
    {
        public string Side { get; private set; }
        public StreamProcessTraceBase(IEnumerable<string> sourceNodeName, string side, TraceLevel level, string message) : base(sourceNodeName, message, level)
        {
            this.Side = side;
        }
        public override string ToString()
        {
            return $"[{string.Join("].[", this.SourceNodeName)}].{this.Side}:({this.Level})-{this.Message}";
        }
    }
}
