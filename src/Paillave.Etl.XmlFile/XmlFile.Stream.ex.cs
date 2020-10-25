using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.XmlFile.Core;

namespace Paillave.Etl.XmlFile
{
    public static class XmlFileEx
    {
        public static IStream<XmlNodeParsed> CrossApplyXmlFile(this IStream<IFileValue> stream, string name, XmlFileDefinition xmlFileDefinition, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider(new XmlFileValuesProviderArgs()
            {
                XmlFileDefinition = xmlFileDefinition,
            });
            return stream.CrossApply<IFileValue, XmlNodeParsed>(name, valuesProvider, noParallelisation);
        }
        public static IStream<T> XmlNodeOfType<T>(this IStream<XmlNodeParsed> stream, string name, string nodeDefinitionName = null)
        {
            return new XmlNodeOfTypeStreamNode<T>(name, new XmlNodeOfTypeFileArgs<T>
            {
                MainStream = stream,
                NodeDefinitionName = nodeDefinitionName
            }).Output;
        }
        public static IStream<Correlated<T>> XmlNodeOfTypeCorrelated<T>(this IStream<XmlNodeParsed> stream, string name, string nodeDefinitionName = null)
        {
            return new XmlNodeOfTypeCorrelatedStreamNode<T>(name, new XmlNodeOfTypeFileArgs<T>
            {
                MainStream = stream,
                NodeDefinitionName = nodeDefinitionName
            }).Output;
        }
    }
}
