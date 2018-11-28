using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.TraceContents
{
    public class NotKeyedStreamTraceContent : StreamTraceContentBase
    {
        public NotKeyedStreamTraceContent(int lineNumber)
        {
            this.LineNumber = lineNumber;
        }
        public int LineNumber { get; }
        public override TraceLevel Level => TraceLevel.Error;
        public override string Message => $"The stream is not keyed at line {LineNumber} whereas it should be";
    }
}
