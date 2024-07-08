using Paillave.Etl.Core;
using Paillave.Etl.Json.Core;

namespace Paillave.Etl.Json
{
    public class JsonFileValuesProviderArgs
    {
        public JsonFileDefinition JsonFileDefinition { get; set; }
        public bool UseStreamCopy { get; set; } = true;
    }
    public class JsonFileValuesProvider : ValuesProviderBase<IFileValue, JsonNodeParsed>
    {
        private JsonFileValuesProviderArgs _args;
        public JsonFileValuesProvider(JsonFileValuesProviderArgs args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public override void PushValues(IFileValue input, Action<JsonNodeParsed> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            using var stream = input.GetContent();
            var jsonObjectReader = new JsonObjectReader(_args.JsonFileDefinition);
            jsonObjectReader.Read(stream, input.Name, push, cancellationToken);
        }
    }
}