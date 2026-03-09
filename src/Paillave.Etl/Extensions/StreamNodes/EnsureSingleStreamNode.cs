using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core;

public class EnsureSingleArgs<T>
{
    public IStream<T> Input { get; set; }
}
public class EnsureSingleStreamNode<TOut>(string name, EnsureSingleArgs<TOut> args) : StreamNodeBase<TOut, ISingleStream<TOut>, EnsureSingleArgs<TOut>>(name, args)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override ISingleStream<TOut> CreateOutputStream(EnsureSingleArgs<TOut> args)
    {
        return base.CreateSingleStream(args.Input.Observable.Map((i, idx) =>
        {
            if (idx > 0) throw new Exception($"{this.NodeName}: There are more than one element in the stream");
            else return i;
        }));
    }
}
