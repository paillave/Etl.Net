using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core;

public class SetForCorrelationArgs<TIn>
{
    public IStream<TIn> Input { get; set; }
}
public class SetForCorrelationStreamNode<TIn>(string name, SetForCorrelationArgs<TIn> args) : StreamNodeBase<Correlated<TIn>, IStream<Correlated<TIn>>, SetForCorrelationArgs<TIn>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<Correlated<TIn>> CreateOutputStream(SetForCorrelationArgs<TIn> args) => 
        base.CreateUnsortedStream(args.Input.Observable.Map(i => new Correlated<TIn> { CorrelationKeys = new HashSet<Guid>(new[] { Guid.NewGuid() }), Row = i }));
}
