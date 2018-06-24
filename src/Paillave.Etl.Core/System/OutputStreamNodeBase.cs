using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class OutputStreamNodeBase<O> : StreamNodeBase
    {
        public OutputStreamNodeBase(ExecutionContextBase context, string name, IEnumerable<string> parentNodeNamePath = null) : base(context, name, parentNodeNamePath)
        {
        }
        public OutputStreamNodeBase(IContextual contextual, string name, IEnumerable<string> parentNodeNamePath = null) : base(contextual.Context, name, parentNodeNamePath)
        {
        }
        protected virtual void CreateOutputStream(IObservable<O> observable)
        {
            this.Output = this.CreateStream<O>(nameof(Output), observable);
        }
        public Stream<O> Output { get; private set; }
    }
}
