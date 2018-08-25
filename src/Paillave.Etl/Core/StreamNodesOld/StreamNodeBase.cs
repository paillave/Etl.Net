using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.Core.StreamNodesOld
{
    public abstract class StreamNodeBase<TStream, TIn> : StreamNodeBase where TStream : IStream<TIn>
    {
        public StreamNodeBase(TStream input, string name)
            : base(input.ExecutionContext, name)
        {
            input.ExecutionContext.AddStreamToNodeLink(new StreamToNodeLink(input.SourceNodeName, input.Name, name));
        }
    }
    public abstract class StreamNodeBase<TStream, TIn, TArgs> : StreamNodeBase<TStream, TIn> where TStream : IStream<TIn>
    {
        public TArgs Arguments { get; private set; }
        public StreamNodeBase(TStream input, string name, TArgs arguments)
            : base(input, name)
        {
            var nodeLinks = GetInputStreamArgumentsLinks(name, arguments);
            foreach (var item in nodeLinks)
                input.ExecutionContext.AddStreamToNodeLink(item);
            this.Arguments = arguments;
        }
    }
    public abstract class StreamNodeBase : INodeContext
    {
        public static List<StreamToNodeLink> GetInputStreamArgumentsLinks<TArgs>(string nodeName, TArgs args)
        {
            return typeof(TArgs)
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetValue(args))
                .OfType<IStream>()
                .Select(stream => new StreamToNodeLink(stream.SourceNodeName, stream.Name, nodeName))
                .ToList();
        }
        protected IExecutionContext ExecutionContext { get; private set; }
        public StreamNodeBase(IExecutionContext executionContext, string name)
        {
            this.ExecutionContext = executionContext;
            this.TypeName = this.GetType().Name;
            this.NodeName = name;
            this.Tracer = new Tracer(executionContext, this);
        }

        public string NodeName { get; private set; }
        public virtual string TypeName { get; private set; }

        protected ITracer Tracer { get; private set; }

        protected IStream<T> CreateStream<T>(string streamName, IPushObservable<T> observable)
        {
            return new Stream<T>(this.Tracer, this.ExecutionContext, this.NodeName, streamName, observable);
        }

        protected ISortedStream<T> CreateSortedStream<T>(string streamName, IPushObservable<T> observable, IEnumerable<ISortCriteria<T>> sortCriterias)
        {
            return new SortedStream<T>(this.Tracer, this.ExecutionContext, this.NodeName, streamName, observable, sortCriterias);
        }
        protected IKeyedStream<T> CreateKeyedStream<T>(string streamName, IPushObservable<T> observable, IEnumerable<ISortCriteria<T>> sortCriterias)
        {
            return new KeyedStream<T>(this.Tracer, this.ExecutionContext, this.NodeName, streamName, observable, sortCriterias);
        }
    }
}
