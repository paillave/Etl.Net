using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Paillave.Etl.Core.TraceContents
{
    public class CancellationTraceContent : StreamTraceContentBase
    {
        private readonly CancellationCause _cause;
        public CancellationTraceContent(CancellationCause cause)
        {
            this._cause = cause;
        }
        public override TraceLevel Level => TraceLevel.Warning;
        private string TextCancellationCause
            => _cause == CancellationCause.CancelledBecauseOfError ? "an error occurred" : "cancellation requested from outside";

        public override string Message => $"Process cancelled: {TextCancellationCause}";
    }
    public enum CancellationCause
    {
        CancelledFromOutside = 1,
        CancelledBecauseOfError = 2
    }
}
