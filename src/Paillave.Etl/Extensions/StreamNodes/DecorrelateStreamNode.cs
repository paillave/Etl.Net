using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core;

public class DecorrelateStreamNodeArgs<TIn>
{
    public IStream<Correlated<TIn>> Input { get; set; }
}
public class DecorrelateStreamNode<TIn> : StreamNodeBase<TIn, IStream<TIn>, DecorrelateStreamNodeArgs<TIn>>
{
    public DecorrelateStreamNode(string name, DecorrelateStreamNodeArgs<TIn> args) : base(name, args)
    {
    }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<TIn> CreateOutputStream(DecorrelateStreamNodeArgs<TIn> args)
    {
        return base.CreateUnsortedStream(args.Input.Observable.Map(i => i.Row));
    }
}
