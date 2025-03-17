using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.JsonFile;

// public class ObjToJsonStreamNode<TIn, TOut>
//     : StreamNodeBase<TOut, IStream<TOut>, JsonArgs<TIn>>
//     where TOut : JObject // Ensure TOut is JObject
public class ObjToJsonStreamNode<TIn>
    : StreamNodeBase<JObject, IStream<JObject>, JsonArgsObjectStream<TIn>>
{
    public ObjToJsonStreamNode(string name, JsonArgsObjectStream<TIn> args)
        : base(name, args) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<JObject> CreateOutputStream(JsonArgsObjectStream<TIn> args)
    {
        // Map from TIn to JObject, and explicitly cast it to TOut (which is JObject)
        var outputObservable = args.Stream.Observable.Map(i =>
            // (TOut)(object)Helpers.ObjectToJson<TIn>(i)
            Helpers.ObjectToJson<TIn>(i)
        );

        return base.CreateUnsortedStream(outputObservable);
    }
}

public class JsonArgsObjectStream<TIn>
{
    public IStream<TIn> Stream { get; set; }
}
