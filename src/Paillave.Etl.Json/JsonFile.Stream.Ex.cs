using Paillave.Etl.Core;
using Paillave.Etl.Json.Core;

namespace Paillave.Etl.Json
{
    public static class JsonFileEx
    {
        public static IStream<JsonNodeParsed> CrossApplyJsonFile(this IStream<IFileValue> stream, string name, Func<JsonFileDefinition, JsonFileDefinition> map, bool noParallelisation = false, bool useStreamCopy = true)
        {
            var valuesProvider = new JsonFileValuesProvider(new JsonFileValuesProviderArgs
            {
                JsonFileDefinition = map(new JsonFileDefinition()),
                UseStreamCopy = useStreamCopy
            });
            return stream.CrossApply<IFileValue, JsonNodeParsed>(name, valuesProvider, noParallelisation);
        }
        public static IStream<JsonNodeParsed> CrossApplyJsonFile(this IStream<IFileValue> stream, string name, JsonFileDefinition jsonFileDefinition, bool noParallelisation = false, bool useStreamCopy = true)
        {
            var valuesProvider = new JsonFileValuesProvider(new JsonFileValuesProviderArgs
            {
                JsonFileDefinition = jsonFileDefinition,
                UseStreamCopy = useStreamCopy
            });
            return stream.CrossApply<IFileValue, JsonNodeParsed>(name, valuesProvider, noParallelisation);
        }
        public static IStream<T> JsonNodeOfType<T>(this IStream<JsonNodeParsed> stream, string name, string nodeDefinitionName = null)
        {
            return new JsonNodeOfTypeStreamNode<T>(name, new JsonNodeOfTypeFileArgs<T>
            {
                MainStream = stream,
                NodeDefinitionName = nodeDefinitionName
            }).Output;
        }
        public static IStream<Correlated<T>> JsonNodeOfTypeCorrelated<T>(this IStream<JsonNodeParsed> stream, string name, string nodeDefinitionName = null)
        {
            return new JsonNodeOfTypeCorrelatedStreamNode<T>(name, new JsonNodeOfTypeFileArgs<T>
            {
                MainStream = stream,
                NodeDefinitionName = nodeDefinitionName
            }).Output;
        }
    }
}
