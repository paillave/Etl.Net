using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace Paillave.Etl.Core.System
{
    public abstract class StreamNodeBase<E> : IAttachable where E : Exception
    {
        private ISubject<ProcessTrace> _traceSubject;
        public StreamNodeBase(ExecutionContextBase traceContext, string name)
        {
            this._traceSubject = new Subject<ProcessTrace>();
            this.Context = traceContext;
            this.Name = name;
            this.AttachToContext();
        }
        protected virtual void AttachToContext()
        {
            this.Context.Attach(this);
        }
        public string Name { get; private set; }
        protected ExecutionContextBase Context { get; private set; }
        public IObservable<ProcessTrace> Trace { get { return this._traceSubject; } }
        protected void AddTrace(ProcessTrace processTrace)
        {
            this._traceSubject.OnNext(processTrace);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this._traceSubject.OnCompleted();
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
