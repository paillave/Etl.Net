using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public abstract class StreamTraceContentBase : ITraceContent
    {
        public StreamTraceContentBase(string streamName)
        {
            this.StreamName = streamName;
        }
        public abstract TraceLevel Level { get; }

        public string StreamName { get; }

        protected abstract string GetMessage();

        public override string ToString() => $".{StreamName}: {this.GetMessage()}";
    }
}
