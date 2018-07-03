using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.SystemOld
{
    public class NotKeyedStreamTraceContent : StreamProcessTraceBase
    {
        public int LineNumber { get; private set; }
        public NotKeyedStreamTraceContent(IEnumerable<string> sourceNodeName, string side, int lineNumber) : base(sourceNodeName, side, TraceLevel.Error, $"The stream is not keyed at line {lineNumber} whereas is should be")
        {
            this.LineNumber = lineNumber;
        }

        public NotKeyedStreamTraceContent(IEnumerable<string> sourceNodeName, string side, TraceLevel level, string message) : base(sourceNodeName, side, level, message)
        {
        }
    }
}
