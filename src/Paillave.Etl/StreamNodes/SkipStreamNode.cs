using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.StreamNodesOld;

namespace Paillave.Etl.StreamNodes
{
    public class SkipStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, int>, IStreamNodeOutput<TIn>
    {
        public IStream<TIn> Output { get; }
        public SkipStreamNode(IStream<TIn> input, string name, int arguments) : base(input, name, arguments)
        {
            this.Output = base.CreateStream(nameof(Output), input.Observable.Skip(arguments));
        }
    }

    public class SkipSortedStreamNode<TIn> : StreamNodeBase<ISortedStream<TIn>, TIn, int>, ISortedStreamNodeOutput<TIn>
    {
        public ISortedStream<TIn> Output { get; }
        public SkipSortedStreamNode(ISortedStream<TIn> input, string name, int arguments) : base(input, name, arguments)
        {
            this.Output = base.CreateSortedStream(nameof(Output), input.Observable.Skip(arguments), input.SortCriterias);
        }
    }

    public class SkipKeyedStreamNode<TIn> : StreamNodeBase<IKeyedStream<TIn>, TIn, int>, IKeyedStreamNodeOutput<TIn>
    {
        public IKeyedStream<TIn> Output { get; }
        public SkipKeyedStreamNode(IKeyedStream<TIn> input, string name, int arguments) : base(input, name, arguments)
        {
            this.Output = base.CreateKeyedStream(nameof(Output), input.Observable.Skip(arguments), input.SortCriterias);
        }
    }
}
