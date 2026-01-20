using Paillave.Etl.Reactive.Operators;
using System;

namespace Paillave.Etl.Core;

public class WhereArgs<TOut, TOutStream> where TOutStream : IStream<TOut>
{
    public TOutStream Input { get; set; }
    public Func<TOut, bool> Predicate { get; set; }
}
public class WhereStreamNode<TOut, TOutStream>(string name, WhereArgs<TOut, TOutStream> args) : StreamNodeBase<TOut, TOutStream, WhereArgs<TOut, TOutStream>>(name, args) where TOutStream : IStream<TOut>
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override TOutStream CreateOutputStream(WhereArgs<TOut, TOutStream> args)
    {
        return base.CreateMatchingStream(args.Input.Observable.Filter(args.Predicate), args.Input);
    }
}
