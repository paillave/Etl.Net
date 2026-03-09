using System;
using System.Collections.Generic;
using System.Linq;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core;

public class ToListArgs<TIn>
{
    public IStream<TIn> InputStream { get; set; }
}
public class ToListStreamNode<TIn>(string name, ToListArgs<TIn> args) : StreamNodeBase<List<TIn>, ISingleStream<List<TIn>>, ToListArgs<TIn>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;
    protected override ISingleStream<List<TIn>> CreateOutputStream(ToListArgs<TIn> args)
        => CreateSingleStream(args.InputStream.Observable.ToList());
}
public class ToListCorrelatedArgs<TIn>
{
    public IStream<Correlated<TIn>> InputStream { get; set; }
}
public class ToListCorrelatedStreamNode<TIn>(string name, ToListCorrelatedArgs<TIn> args) : StreamNodeBase<Correlated<List<TIn>>, ISingleStream<Correlated<List<TIn>>>, ToListCorrelatedArgs<TIn>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;
    protected override ISingleStream<Correlated<List<TIn>>> CreateOutputStream(ToListCorrelatedArgs<TIn> args)
        => CreateSingleStream(args.InputStream.Observable.ToList().Map(i => new Correlated<List<TIn>>
        {
            CorrelationKeys = i.SelectMany(j => j.CorrelationKeys).ToHashSet(),
            Row = i.Select(j => j.Row).ToList()
        }));
}
