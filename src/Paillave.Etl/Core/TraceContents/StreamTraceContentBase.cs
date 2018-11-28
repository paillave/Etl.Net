using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public abstract class StreamTraceContentBase : ITraceContent
    {
        public virtual string Type => GetType().Name;
        public abstract TraceLevel Level { get; }
        public abstract string Message { get; }
        public override string ToString() => this.Message;
    }
}
