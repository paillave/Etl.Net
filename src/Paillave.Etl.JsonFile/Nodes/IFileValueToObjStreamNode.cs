using System;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.JsonFile;

// public class IFileValueToObjStreamNode<TIn, TOut>
//     : StreamNodeBase<TOut, IStream<TOut>, JsonArgsIFileValueStream<TIn>>
//     where TIn : IFileValue

public class IFileValueToObjStreamNode<TOut>
    : StreamNodeBase<TOut, IStream<TOut>, JsonArgsIFileValueStream<IFileValue>>

{
    public IFileValueToObjStreamNode(string name, JsonArgsIFileValueStream<IFileValue> args)
        : base(name, args) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<TOut> CreateOutputStream(JsonArgsIFileValueStream<IFileValue> args)
    {
        var outputObservable = args.Stream.Observable.Map(i => Helpers.FileValueToObject<TOut>(i));

        return base.CreateUnsortedStream(outputObservable);
    }
}

public class JsonArgsIFileValueStream<TIn>
{
    public IStream<IFileValue> Stream { get; set; }
}
