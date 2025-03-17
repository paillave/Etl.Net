using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.JsonFile;

public class ObjToJsonFileValueStreamNode<TIn>
    : StreamNodeBase<JsonFileValue, IStream<JsonFileValue>, JsonArgsObjectStream<TIn>>
{
    public ObjToJsonFileValueStreamNode(string name, JsonArgsObjectStream<TIn> args)
        : base(name, args) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<JsonFileValue> CreateOutputStream(JsonArgsObjectStream<TIn> args)
    {
        // Map from TIn to JObject, and explicitly cast it to TOut (which is JObject)
        var outputObservable = args.Stream.Observable.Map(i =>
        {
            var payload = Helpers.ObjectToJson<TIn>(i);
            return new JsonFileValue(NodeName, payload);
        });

        return base.CreateUnsortedStream(outputObservable);
    }
}

public class JsonArgsObjectStream<TIn>
{
    public IStream<TIn> Stream { get; set; }
}
