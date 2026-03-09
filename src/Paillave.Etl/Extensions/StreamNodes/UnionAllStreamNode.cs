using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core;

public class UnionAllArgs<TIn>
{
    public IStream<TIn> Stream1 { get; set; }
    public IStream<TIn> Stream2 { get; set; }
}
public class UnionAllStreamNode<TIn>(string name, UnionAllArgs<TIn> args) : StreamNodeBase<TIn, IStream<TIn>, UnionAllArgs<TIn>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;

    protected override IStream<TIn> CreateOutputStream(UnionAllArgs<TIn> args)
    {
        return base.CreateUnsortedStream(args.Stream1.Observable.Concatenate(args.Stream2.Observable));
    }
}
