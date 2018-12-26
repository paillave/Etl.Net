using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public class UnhandledExceptionStreamTraceContent : StreamTraceContentBase
    {
        public UnhandledExceptionStreamTraceContent(Exception ex)
        {
            this.Exception = ex;
        }

        public override TraceLevel Level => TraceLevel.Error;

        public Exception Exception { get; }

        public override string Message => $"Unhandled exception: {this.Exception.Message}";
    }
}
