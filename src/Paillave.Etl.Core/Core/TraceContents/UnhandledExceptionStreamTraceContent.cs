using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public class UnhandledExceptionStreamTraceContent : StreamTraceContentBase
    {
        public UnhandledExceptionStreamTraceContent(string outputName, Exception ex) : base(outputName)
        {
            this.Exception = ex;
        }

        public override TraceLevel Level => TraceLevel.Error;

        public Exception Exception { get; }

        protected override string GetMessage() => $"Unhandled exception: {this.Exception.Message}";
    }
}
