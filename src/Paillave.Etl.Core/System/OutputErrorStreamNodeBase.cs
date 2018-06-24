using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class OutputErrorStreamNodeBase<O, E> : OutputStreamNodeBase<O>
    {
        public OutputErrorStreamNodeBase(ExecutionContextBase context, string name, IEnumerable<string> parentNodeNamePath = null) : base(context, name, parentNodeNamePath)
        {
        }
        public OutputErrorStreamNodeBase(IContextual contextual, string name, IEnumerable<string> parentNodeNamePath = null) : base(contextual.Context, name, parentNodeNamePath)
        {
        }
        protected virtual void CreateErrorStream(IObservable<E> observable)
        {
            this.Error = this.CreateStream<E>(nameof(Error), observable);
        }
        public Stream<E> Error { get; private set; }
    }
}
