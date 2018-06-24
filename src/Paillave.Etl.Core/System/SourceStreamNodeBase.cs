using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class SourceStreamNodeBase<O> : OutputStreamNodeBase<O>, ISourceStreamNode
    {
        private ISubject<O> _outputSubject;

        public SourceStreamNodeBase(ExecutionContextBase traceContext, string name, IEnumerable<string> parentsName = null) : base(traceContext, name, parentsName)
        {
            var sourceNodeNames = (parentsName ?? new string[] { }).Concat(new[] { name }).ToList();
            this._outputSubject = new Subject<O>();
            this.CreateOutputStream(this._outputSubject);
            traceContext.Attach(this);
        }

        protected void OnNextOutput(O output)
        {
            this._outputSubject.OnNext(output);
        }

        protected void OnCompleted()
        {
            this._outputSubject.OnCompleted();
        }

        public abstract void Start();
    }
}
