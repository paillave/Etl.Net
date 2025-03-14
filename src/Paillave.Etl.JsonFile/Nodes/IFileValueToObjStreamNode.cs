using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.JsonFile;

public class IFileValueToObjStreamNode<TIn, TOut>
    : StreamNodeBase<TOut, IStream<TOut>, JsonArgs<TIn>>
{
    public IFileValueToObjStreamNode(string name, JsonArgs<TIn> args)
        : base(name, args) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<TOut> CreateOutputStream(JsonArgs<TIn> args)
    {
        var outputObservable = args.Stream.Observable.Map(i =>
            Helpers.GetObjectFromFileValue<TOut>(i)
        );

        return base.CreateUnsortedStream(outputObservable);
    }
}

public class JsonArgs<TIn>
{
    public Stream<TIn> Stream { get; set; }
}
