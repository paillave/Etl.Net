using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Json.Core;

namespace Paillave.Etl.Json
{
    public class JsonNodeOfTypeFileArgs<TOut>
    {
        public IStream<JsonNodeParsed> MainStream { get; set; }
        public string NodeDefinitionName { get; set; }
    }
    public class JsonNodeOfTypeStreamNode<TOut> : StreamNodeBase<TOut, IStream<TOut>, JsonNodeOfTypeFileArgs<TOut>>
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public JsonNodeOfTypeStreamNode(string name, JsonNodeOfTypeFileArgs<TOut> args) : base(name, args) { }
        protected override IStream<TOut> CreateOutputStream(JsonNodeOfTypeFileArgs<TOut> args)
        {
            var type = typeof(TOut);
            var obs = args.MainStream.Observable.Filter(i => i.Type == type);
            if (args.NodeDefinitionName != null)
                obs = obs.Filter(i => i.NodeDefinitionName == args.NodeDefinitionName);
            return CreateUnsortedStream(obs.Map(i => (TOut)i.Value));
        }
    }
    public class JsonNodeOfTypeCorrelatedStreamNode<TOut> : StreamNodeBase<Correlated<TOut>, IStream<Correlated<TOut>>, JsonNodeOfTypeFileArgs<TOut>>
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public JsonNodeOfTypeCorrelatedStreamNode(string name, JsonNodeOfTypeFileArgs<TOut> args) : base(name, args) { }
        protected override IStream<Correlated<TOut>> CreateOutputStream(JsonNodeOfTypeFileArgs<TOut> args)
        {
            var type = typeof(TOut);
            var obs = args.MainStream.Observable.Filter(i => i.Type == type);
            if (args.NodeDefinitionName != null)
                obs = obs.Filter(i => i.NodeDefinitionName == args.NodeDefinitionName);
            return CreateUnsortedStream(obs.Map(i => new Correlated<TOut>
            {
                CorrelationKeys = i.CorrelationKeys,
                Row = (TOut)i.Value
            }));
        }
    }
}
