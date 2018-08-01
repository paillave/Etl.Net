using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System.TraceContents
{
    public class NotKeyedStreamTraceContent : StreamTraceContentBase
    {
        public NotKeyedStreamTraceContent(string outputName, int lineNumber) : base(outputName)
        {
            this.LineNumber = lineNumber;
        }

        public int LineNumber { get; }

        public override TraceLevel Level => TraceLevel.Error;

        protected override string GetMessage() => $"The stream is not keyed at line {LineNumber} whereas it should be";
    }
}
