using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class StreamNodeArgs
    {
        private IEnumerable<IStream> GetStreams()
        {
            return GetType()
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetValue(this))
                .OfType<IStream>();
        }
        public List<StreamToNodeLink> GetInputStreamArgumentsLinks(string nodeName)
        {
            return GetStreams()
                .Select(stream => new StreamToNodeLink(stream.SourceNodeName, stream.Name, nodeName))
                .ToList();
        }
        public IExecutionContext GetExecutionContext()
        {
            return this.GetStreams()
                .Select(i => i.ExecutionContext)
                .FirstOrDefault(i => !i.IsTracingContext);
        }
    }
    public abstract class StreamNodeBase<TOut, TArgs> : INodeContext where TArgs : StreamNodeArgs
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
