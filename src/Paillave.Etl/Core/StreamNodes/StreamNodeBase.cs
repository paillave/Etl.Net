using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public abstract class StreamNodeBase<TOut, TArgs> : INodeContext
    where TArgs : StreamNodeArgs
    {
        protected IExecutionContext ExecutionContext { get; }
        protected ITracer Tracer { get; private set; }
        public string NodeName { get; private set; }
        public virtual string TypeName { get; private set; }

        public StreamNodeBase(string name, TArgs args)
        {
            this.TypeName = this.GetType().Name;
            this.NodeName = name;
            this.ExecutionContext = args.GetExecutionContext();
            this.Tracer = new Tracer(this.ExecutionContext, this);
        }
    }
}
