using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.XmlFile.Core;
using Paillave.Etl.XmlFile.ValuesProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Paillave.Etl.XmlFile.Extensions
{
    public static class XmlFileEx
    {
        public static IStream<TOut> CrossApplyXmlFile<TOut>(this IStream<string> stream, string name, XmlNodeDefinition<TOut> xmlNodeDefinitions, IDictionary<string, string> prefixToUriNamespaces = null, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<string, TOut, TOut>(new XmlFileValuesProviderArgs<string, TOut, TOut>()
            {
                PrefixToUriNamespaces = prefixToUriNamespaces,
                DataStreamSelector = i => File.OpenRead(i),
                XmlNodeDefinition = xmlNodeDefinitions,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<string, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyXmlFile<TOut>(this IStream<LocalFilesValue> stream, string name, XmlNodeDefinition<TOut> xmlNodeDefinitions, IDictionary<string, string> prefixToUriNamespaces = null, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<LocalFilesValue, TOut, TOut>(new XmlFileValuesProviderArgs<LocalFilesValue, TOut, TOut>()
            {
                PrefixToUriNamespaces = prefixToUriNamespaces,
                DataStreamSelector = i => i.GetContent(),
                XmlNodeDefinition = xmlNodeDefinitions,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<LocalFilesValue, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyXmlFile<TOut>(this IStream<Stream> stream, string name, XmlNodeDefinition<TOut> xmlNodeDefinitions, IDictionary<string, string> prefixToUriNamespaces = null, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<Stream, TOut, TOut>(new XmlFileValuesProviderArgs<Stream, TOut, TOut>()
            {
                PrefixToUriNamespaces = prefixToUriNamespaces,
                DataStreamSelector = i => i,
                XmlNodeDefinition = xmlNodeDefinitions,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<Stream, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyXmlFile<TIn, TOut>(this IStream<TIn> stream, string name, XmlNodeDefinition<TOut> xmlNodeDefinitions, Func<TIn, string> filePathSelector, IDictionary<string, string> prefixToUriNamespaces = null, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<TIn, TOut, TOut>(new XmlFileValuesProviderArgs<TIn, TOut, TOut>()
            {
                PrefixToUriNamespaces = prefixToUriNamespaces,
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                XmlNodeDefinition = xmlNodeDefinitions,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<TIn, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyXmlFile<TIn, TParsed, TOut>(this IStream<TIn> stream, string name, XmlNodeDefinition<TParsed> xmlNodeDefinitions, Func<TIn, string> filePathSelector, Func<TIn, TParsed, TOut> resultSelector, IDictionary<string, string> prefixToUriNamespaces = null, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<TIn, TParsed, TOut>(new XmlFileValuesProviderArgs<TIn, TParsed, TOut>()
            {
                PrefixToUriNamespaces = prefixToUriNamespaces,
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                XmlNodeDefinition = xmlNodeDefinitions,
                ResultSelector = resultSelector
            });
            return stream.CrossApply<TIn, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyXmlFile<TParsed, TOut>(this IStream<string> stream, string name, XmlNodeDefinition<TParsed> xmlNodeDefinitions, Func<string, TParsed, TOut> resultSelector, IDictionary<string, string> prefixToUriNamespaces = null, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<string, TParsed, TOut>(new XmlFileValuesProviderArgs<string, TParsed, TOut>()
            {
                PrefixToUriNamespaces = prefixToUriNamespaces,
                DataStreamSelector = i => File.OpenRead(i),
                XmlNodeDefinition = xmlNodeDefinitions,
                ResultSelector = resultSelector
            });
            return stream.CrossApply<string, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyXmlFile<TParsed, TOut>(this IStream<Stream> stream, string name, XmlNodeDefinition<TParsed> xmlNodeDefinitions, Func<TParsed, TOut> resultSelector, IDictionary<string, string> prefixToUriNamespaces = null, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<Stream, TParsed, TOut>(new XmlFileValuesProviderArgs<Stream, TParsed, TOut>()
            {
                PrefixToUriNamespaces = prefixToUriNamespaces,
                DataStreamSelector = i => i,
                XmlNodeDefinition = xmlNodeDefinitions,
                ResultSelector = (s, o) => resultSelector(o)
            });
            return stream.CrossApply<Stream, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }

        public static IStream<XmlNodeParsed> CrossApplyXmlFile(this IStream<string> stream, string name, XmlFileDefinition xmlFileDefinition, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<string, XmlNodeParsed>(new XmlFileValuesProviderArgs<string, XmlNodeParsed>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                XmlFileDefinition = xmlFileDefinition,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<string, XmlNodeParsed>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<XmlNodeParsed> CrossApplyXmlFile(this IStream<LocalFilesValue> stream, string name, XmlFileDefinition xmlFileDefinition, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<LocalFilesValue, XmlNodeParsed>(new XmlFileValuesProviderArgs<LocalFilesValue, XmlNodeParsed>()
            {
                DataStreamSelector = i => i.GetContent(),
                XmlFileDefinition = xmlFileDefinition,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<LocalFilesValue, XmlNodeParsed>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<XmlNodeParsed> CrossApplyXmlFile(this IStream<Stream> stream, string name, XmlFileDefinition xmlFileDefinition, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<Stream, XmlNodeParsed>(new XmlFileValuesProviderArgs<Stream, XmlNodeParsed>()
            {
                DataStreamSelector = i => i,
                XmlFileDefinition = xmlFileDefinition,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<Stream, XmlNodeParsed>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<XmlNodeParsed> CrossApplyXmlFile<TIn>(this IStream<TIn> stream, string name, XmlFileDefinition xmlFileDefinition, Func<TIn, string> filePathSelector, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<TIn, XmlNodeParsed>(new XmlFileValuesProviderArgs<TIn, XmlNodeParsed>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                XmlFileDefinition = xmlFileDefinition,
                ResultSelector = (i, o) => o
            });
            return stream.CrossApply<TIn, XmlNodeParsed>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyXmlFile<TIn, TOut>(this IStream<TIn> stream, string name, XmlFileDefinition xmlFileDefinition, Func<TIn, string> filePathSelector, Func<TIn, XmlNodeParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<TIn, TOut>(new XmlFileValuesProviderArgs<TIn, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(filePathSelector(i)),
                XmlFileDefinition = xmlFileDefinition,
                ResultSelector = resultSelector
            });
            return stream.CrossApply<TIn, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyXmlFile<TOut>(this IStream<string> stream, string name, XmlFileDefinition xmlFileDefinition, Func<string, XmlNodeParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<string, TOut>(new XmlFileValuesProviderArgs<string, TOut>()
            {
                DataStreamSelector = i => File.OpenRead(i),
                XmlFileDefinition = xmlFileDefinition,
                ResultSelector = resultSelector
            });
            return stream.CrossApply<string, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
        public static IStream<TOut> CrossApplyXmlFile<TOut>(this IStream<Stream> stream, string name, XmlFileDefinition xmlFileDefinition, Func<XmlNodeParsed, TOut> resultSelector, bool noParallelisation = false)
        {
            var valuesProvider = new XmlFileValuesProvider<Stream, TOut>(new XmlFileValuesProviderArgs<Stream, TOut>()
            {
                DataStreamSelector = i => i,
                XmlFileDefinition = xmlFileDefinition,
                ResultSelector = (s, o) => resultSelector(o)
            });
            return stream.CrossApply<Stream, TOut>(name, valuesProvider.PushValues, noParallelisation);
        }
    }
}
