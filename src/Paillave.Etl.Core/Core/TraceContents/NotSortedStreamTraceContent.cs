using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.TraceContents
{
    public class NotSortedStreamTraceContent : StreamTraceContentBase
    {
        public NotSortedStreamTraceContent(string outputName, int lineNumber) : base(outputName)
        {
            this.LineNumber = lineNumber;
        }

        public int LineNumber { get; }

        public override TraceLevel Level => TraceLevel.Error;

        protected override string GetMessage() => $"The stream is not sorted at line {LineNumber} whereas it should be";
    }
}
