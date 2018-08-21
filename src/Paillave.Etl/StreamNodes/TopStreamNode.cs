using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{
    public class TopStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, int>, IStreamNodeOutput<TIn>
    {
        public IStream<TIn> Output { get; }
        public TopStreamNode(IStream<TIn> input, string name, int arguments) : base(input, name, arguments)
        {
            this.Output = base.CreateStream(nameof(Output), input.Observable.Take(arguments));
        }
    }
    public class TopSortedStreamNode<TIn> : StreamNodeBase<ISortedStream<TIn>, TIn, int>, ISortedStreamNodeOutput<TIn>
    {
        public ISortedStream<TIn> Output { get; }
        public TopSortedStreamNode(ISortedStream<TIn> input, string name, int arguments) : base(input, name, arguments)
        {
            this.Output = base.CreateSortedStream(nameof(Output), input.Observable.Take(arguments), input.SortCriterias);
        }
    }
    public class TopKeyedStreamNode<TIn> : StreamNodeBase<IKeyedStream<TIn>, TIn, int>, IKeyedStreamNodeOutput<TIn>
    {
        public IKeyedStream<TIn> Output { get; }
        public TopKeyedStreamNode(IKeyedStream<TIn> input, string name, int arguments) : base(input, name, arguments)
        {
            this.Output = base.CreateKeyedStream(nameof(Output), input.Observable.Take(arguments), input.SortCriterias);
        }
    }
}
