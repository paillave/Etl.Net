using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class SourceStreamNodeBase<O, E> : StreamNodeBase, ISourceStreamNode
        where E : Exception
    {
        private ISubject<O> _outputSubject;
        private ISubject<E> _errorSubject;
        public SourceStreamNodeBase(ExecutionContextBase traceContext, string name, IEnumerable<string> parentsName = null) : base(traceContext, name)
        {
            var sourceNodeNames = (parentsName ?? new string[] { }).Concat(new[] { name }).ToList();
            this._outputSubject = new Subject<O>();
            this._errorSubject = new Subject<E>();
            this.OutputStream = new Stream<O>(traceContext, sourceNodeNames, nameof(OutputStream), this._outputSubject);
            this.ErrorStream = new Stream<E>(traceContext, sourceNodeNames, nameof(ErrorStream), this._errorSubject);
            traceContext.Attach(this);
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
