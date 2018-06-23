using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class StreamTraceExecutionContext : ExecutionContextBase
    {
        private ISubject<ProcessTraceContext> _subject;
        private readonly SortedStream<ProcessTraceContext> _processTraceContextStream;

        public StreamTraceExecutionContext()
        {
            this._subject = new Subject<ProcessTraceContext>();
            this._processTraceContextStream = new SortedStream<ProcessTraceContext>(new NullTraceContext(), null, null, this._subject, new SortCriteria<ProcessTraceContext>(i => i.DateTime));
        }
        public Stream<ProcessTraceContext> ProcessTraceStream { get { return this._processTraceContextStream; } }
        public override void OnNextProcessTrace(ProcessTrace processTrace)
        {
            this._subject.OnNext(new ProcessTraceContext(this.ExecutionId, processTrace));
        }
        private bool _disposedValue = false;
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!_disposedValue)
            {
                if (disposing)
                {
                    this._subject.OnCompleted();
                }
                _disposedValue = true;
            }
        }
    }
}
