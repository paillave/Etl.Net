using System;
using Paillave.Etl.Core;
using Paillave.Etl.XmlFile.Core;

namespace Paillave.Etl.XmlFile;

public static class XmlFileEx
{
    public static IStream<XmlNodeParsed> CrossApplyXmlFile(this IStream<IFileValue> stream, string name, Func<XmlFileDefinition, XmlFileDefinition> map, bool noParallelisation = false, bool useStreamCopy = true)
    {
        var valuesProvider = new XmlFileValuesProvider(new XmlFileValuesProviderArgs
        {
            XmlFileDefinition = map(new XmlFileDefinition()),
            UseStreamCopy = useStreamCopy
        });
        return stream.CrossApply<IFileValue, XmlNodeParsed>(name, valuesProvider, noParallelisation);
    }
    public static IStream<XmlNodeParsed> CrossApplyXmlFile(this IStream<IFileValue> stream, string name, XmlFileDefinition xmlFileDefinition, bool noParallelisation = false, bool useStreamCopy = true)
    {
        var valuesProvider = new XmlFileValuesProvider(new XmlFileValuesProviderArgs
        {
            XmlFileDefinition = xmlFileDefinition,
            UseStreamCopy = useStreamCopy
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
    public static IStream<Correlated<T>> XmlNodeOfTypeCorrelated<T>(this IStream<XmlNodeParsed> stream, string name, string correlationPath, string? nodeDefinitionName = null)
    {
        return new XmlNodeOfTypeCorrelatedStreamNode<T>(name, new XmlNodeOfTypeCorrelatedFileArgs<T>
        {
            MainStream = stream,
            NodeDefinitionName = nodeDefinitionName,
            CorrelationPath = correlationPath
        }).Output;
    }
}
