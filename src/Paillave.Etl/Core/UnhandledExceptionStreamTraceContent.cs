using System;
using System.Diagnostics;

namespace Paillave.Etl.Core
{
    public class UnhandledExceptionStreamTraceContent : StreamTraceContentBase
    {
        public UnhandledExceptionStreamTraceContent(Exception ex) => (Exception) = (ex);
        public override TraceLevel Level => TraceLevel.Error;
        public Exception Exception { get; }
        public override string Message => $"Unhandled exception: {this.Exception.GetFullMessage()}";
    }
}
