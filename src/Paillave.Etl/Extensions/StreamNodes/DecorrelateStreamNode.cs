using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core;

public class DecorrelateStreamNodeArgs<TIn>
{
    public IStream<Correlated<TIn>> Input { get; set; }
}
public class DecorrelateStreamNode<TIn>(string name, DecorrelateStreamNodeArgs<TIn> args) : StreamNodeBase<TIn, IStream<TIn>, DecorrelateStreamNodeArgs<TIn>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<TIn> CreateOutputStream(DecorrelateStreamNodeArgs<TIn> args) =>
        base.CreateUnsortedStream(args.Input.Observable.Map(i => i.Row));
}
