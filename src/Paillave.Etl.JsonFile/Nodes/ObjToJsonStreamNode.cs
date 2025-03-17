using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.JsonFile;

public class ObjToJsonStreamNode<TIn>
    : StreamNodeBase<JObject, IStream<JObject>, JsonArgsObjectStream<TIn>>
{
    public ObjToJsonStreamNode(string name, JsonArgsObjectStream<TIn> args)
        : base(name, args) { }

    public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    protected override IStream<JObject> CreateOutputStream(JsonArgsObjectStream<TIn> args)
    {
        var outputObservable = args.Stream.Observable.Map(i =>
            Helpers.ObjectToJson<TIn>(i)
        );

        return base.CreateUnsortedStream(outputObservable);
    }
}

