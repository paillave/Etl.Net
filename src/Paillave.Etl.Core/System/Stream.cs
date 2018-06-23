using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;


namespace Paillave.Etl.Core.System
{
    public class Stream<T> : IAttachable
    {
        public T CurrentValue { get; private set; } = default(T);
        private int _counter = 0;
        protected ISubject<ProcessTrace> TraceSubject { get; private set; }
        private ISubject<T> _observableCopyIfUsed = new Subject<T>();
        public Stream(ExecutionContextBase traceContext, string sourceNodeName, string sourceOutputName, IObservable<T> observable)
        {
            this.TraceSubject = new Subject<ProcessTrace>();
            this.Observable = observable
                .Do(e =>
                {
                    this._observableCopyIfUsed.OnNext(e);
                    this.CurrentValue = e;
                    TraceSubject.OnNext(new CounterProcessTrace(sourceNodeName, sourceOutputName, ++this._counter));
                });
            this.SourceNodeName = sourceNodeName;
            this.SourceOutputName = sourceOutputName;
            traceContext.Attach(this);
        }
        protected IObservable<T> ObservableCopyIfUsed { get { return this._observableCopyIfUsed; } }
        public string SourceNodeName { get; private set; }
        public string SourceOutputName { get; private set; }
        public IObservable<T> Observable { get; private set; }
        public IObservable<ProcessTrace> Trace { get { return this.TraceSubject; } }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.TraceSubject.OnNext(new CounterSummaryProcessTrace(this.SourceNodeName, this.SourceOutputName, this._counter));
                    this.TraceSubject.OnCompleted();
                    this._observableCopyIfUsed.OnCompleted();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
