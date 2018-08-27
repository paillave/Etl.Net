using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public abstract class StreamTraceContentBase : ITraceContent
    {
        public abstract TraceLevel Level { get; }

        protected abstract string GetMessage();

        public override string ToString() => $": {this.GetMessage()}";
    }
}
