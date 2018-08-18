using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public class RowProcessStreamTraceContent : StreamTraceContentBase
    {
        public RowProcessStreamTraceContent(string streamName, int position, object row) : base(streamName)
        {
            this.Position = position;
            this.Row = row;
        }
        public override TraceLevel Level => TraceLevel.Verbose;

        public int Position { get; }
        public object Row { get; }

        protected override string GetMessage() => $"{Row} row(s) processing";
    }
}
