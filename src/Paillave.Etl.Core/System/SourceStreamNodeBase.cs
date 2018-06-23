using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class SourceStreamNodeBase<O, E> : StreamNodeBase<E>, ISourceStreamNode
        where E : Exception
    {
        private ISubject<O> _outputSubject;
        private ISubject<E> _errorSubject;
        public SourceStreamNodeBase(ExecutionContextBase traceContext, string name) : base(traceContext, name)
        {
            this._outputSubject = new Subject<O>();
            this._errorSubject = new Subject<E>();
            this.OutputStream = new Stream<O>(traceContext, name, nameof(OutputStream), this._outputSubject);
            this.ErrorStream = new Stream<E>(traceContext, name, nameof(ErrorStream), this._errorSubject);
        }
        protected override void AttachToContext()
        {
            this.Context.Attach(this);
        }
        public Stream<O> OutputStream { get; private set; }
        public Stream<E> ErrorStream { get; private set; }

        protected void OnNextOutput(O output)
        {
            this._outputSubject.OnNext(output);
        }
        protected void OnNextError(E error)
        {
            this._errorSubject.OnNext(error);
        }

        protected void OnCompleted()
        {
            this._outputSubject.OnCompleted();
            this._errorSubject.OnCompleted();
        }

        public abstract void Start();
    }
}
