using System;
using System.Collections.Generic;
using System.Linq;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    public class WithPreviousArgs<TIn, TOut>
    {
        public int Count { get; set; }
        public IStream<TIn> Stream { get; set; }
        public Func<TIn[], TOut> GetResult { get; set; }
    }
    public class WithPreviousStreamNode<TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, WithPreviousArgs<TIn, TOut>>
    {
        public WithPreviousStreamNode(string name, WithPreviousArgs<TIn, TOut> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override IStream<TOut> CreateOutputStream(WithPreviousArgs<TIn, TOut> args)
        {
            var obs = args.Stream.Observable
                .Scan<TIn, (FixedQueue<TIn> Queue, TIn[] Items)>((new FixedQueue<TIn>(args.Count), new TIn[] { }), (a, v) =>
                    {
                        a.Queue.Enqueue(v);
                        return (a.Queue, a.Queue.ToArray().Reverse().ToArray());
                    })
                .Map(i => args.GetResult(i.Items));
            return base.CreateUnsortedStream(obs);
        }
    }
    public class WithPreviousCorrelatedArgs<TIn, TOut>
    {
        public int Count { get; set; }
        public IStream<Correlated<TIn>> Stream { get; set; }
        public Func<TIn[], TOut> GetResult { get; set; }
    }
    public class WithPreviousCorrelatedStreamNode<TIn, TOut> : StreamNodeBase<Correlated<TOut>, IStream<Correlated<TOut>>, WithPreviousCorrelatedArgs<TIn, TOut>>
    {
        public WithPreviousCorrelatedStreamNode(string name, WithPreviousCorrelatedArgs<TIn, TOut> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override IStream<Correlated<TOut>> CreateOutputStream(WithPreviousCorrelatedArgs<TIn, TOut> args)
        {
            var obs = args.Stream.Observable
                .Scan<Correlated<TIn>, (FixedQueue<Correlated<TIn>> Queue, TIn[] Items, HashSet<Guid> CorrelationKeys)>((new FixedQueue<Correlated<TIn>>(args.Count), new TIn[] { }, new HashSet<Guid>()), (a, v) =>
                    {
                        a.Queue.Enqueue(v);
                        return (
                                a.Queue,
                                a.Queue.ToArray().Reverse().Select(i => i.Row).ToArray(),
                                a.Queue.ToArray().SelectMany(i => i.CorrelationKeys).ToHashSet());
                    })
                .Map(i => new Correlated<TOut>
                {
                    Row = args.GetResult(i.Items),
                    CorrelationKeys = i.CorrelationKeys
                });
            return base.CreateUnsortedStream(obs);
        }
    }
}