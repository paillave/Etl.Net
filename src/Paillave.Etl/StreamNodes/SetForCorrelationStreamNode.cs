using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class SetForCorrelationArgs<TIn>
    {
        public IStream<TIn> Input { get; set; }
    }
    public class SetForCorrelationStreamNode<TIn> : StreamNodeBase<Correlated<TIn>, IStream<Correlated<TIn>>, SetForCorrelationArgs<TIn>>
    {
        public SetForCorrelationStreamNode(string name, SetForCorrelationArgs<TIn> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<Correlated<TIn>> CreateOutputStream(SetForCorrelationArgs<TIn> args)
        {
            return base.CreateUnsortedStream(args.Input.Observable.Map(i => new Correlated<TIn> { CorrelationKeys = new HashSet<Guid>(new[] { Guid.NewGuid() }), Row = i }));
        }
    }
}
