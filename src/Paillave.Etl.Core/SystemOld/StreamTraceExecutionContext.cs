using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.SystemOld
{
    public class StreamTraceExecutionContext : ExecutionContext, IDisposable
    {
        private ISubject<ProcessTraceContext> _subject;
        private readonly SortedStream<ProcessTraceContext> _processTraceContextStream;
        public IStream<ProcessTraceContext> ProcessTraceStream { get { return this._processTraceContextStream; } }

        public StreamTraceExecutionContext()
        {
            this._subject = new Subject<ProcessTraceContext>();
            this._processTraceContextStream = new SortedStream<ProcessTraceContext>(null, null, null, this._subject, new[] { new SortCriteria<ProcessTraceContext>(i => i.DateTime) });
        }
        public override void OnNextProcessTrace(ProcessTrace processTrace)
        {
            this._subject.OnNext(new ProcessTraceContext(this.ExecutionId, processTrace));
        }

        #region IDisposable Support
        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this._subject.OnCompleted();
                }
                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }

        public override void OnNextExceptionProcessTrace(ExceptionProcessTrace processTrace)
        {
            this._subject.OnNext(new ExceptionProcessTraceContext(this.ExecutionId, processTrace));
        }
        #endregion
    }
}
