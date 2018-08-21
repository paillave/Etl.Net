using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{
    public class UnionArgs<TIn>
    {
        public IStream<TIn> SecondStream { get; set; }
    }
    public class UnionStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, UnionArgs<TIn>>, IStreamNodeOutput<TIn>
    {
        public IStream<TIn> Output { get; }
        public UnionStreamNode(IStream<TIn> input, string name, UnionArgs<TIn> arguments) : base(input, name, arguments)
        {
            this.Output = base.CreateStream(nameof(Output), input.Observable.Merge(arguments.SecondStream.Observable));
        }
    }
}
