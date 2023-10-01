using System.Diagnostics;

namespace Paillave.Etl.Core
{
    public abstract class StreamTraceContentBase : ITraceContent
    {
        public virtual string Type => GetType().Name;
        public abstract TraceLevel Level { get; }
        public abstract string Message { get; }
        public override string ToString() => this.Message;
    }
}
