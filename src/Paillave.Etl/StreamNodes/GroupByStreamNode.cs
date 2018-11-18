using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.StreamNodes
{
    public class GroupArgs<TIn, TKey, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public Func<TIn, TKey> GetKey { get; set; }
        public Func<IStream<TIn>, IStream<TOut>> SubProcess { get; set; }
    }
    public class GroupByStreamNode<TIn, TKey, TOut> : StreamNodeBase<TOut, IStream<TOut>, GroupArgs<TIn, TKey, TOut>>
    {
        public override bool IsAwaitable => true;
        public GroupByStreamNode(string name, GroupArgs<TIn, TKey, TOut> args) : base(name, args)
        {
        }
        protected override IStream<TOut> CreateOutputStream(GroupArgs<TIn, TKey, TOut> args)
        {
            var outputObservable = args.Stream.Observable.Group(args.GetKey, iS => args.SubProcess(new Stream<TIn>(this.Tracer.GetSubTracer(this), this.ExecutionContext, this.NodeName, iS)).Observable);
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
