using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.System
{
    public abstract class StreamTraceContentBase : ITraceContent
    {
        public StreamTraceContentBase(string outputName)
        {
            this.OutputName = outputName;
        }
        public abstract TraceLevel Level { get; }

        public string OutputName { get; }

        protected abstract string GetMessage();

        public override string ToString() => $"({OutputName})-{this.GetMessage()}";
    }
}
