using Paillave.Etl.XmlFile.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Paillave.Etl.XmlFile.ValuesProviders
{
    public class XmlFileValuesProviderArgs<TIn, TParsed, TOut>
    {
        public IDictionary<string, string> PrefixToUriNamespaces { get; set; }
        public XmlNodeDefinition<TParsed> XmlNodeDefinition { get; set; }
        public Func<TIn, TParsed, TOut> ResultSelector { get; set; }
        public Func<TIn, Stream> DataStreamSelector { get; set; }
    }
    public class XmlFileValuesProviderArgs<TIn, TOut>
    {
        public XmlFileDefinition XmlFileDefinition { get; set; }
        public Func<TIn, XmlNodeParsed, TOut> ResultSelector { get; set; }
        public Func<TIn, Stream> DataStreamSelector { get; set; }
    }
    public class XmlFileValuesProvider<TIn, TParsed, TOut>
    {
        private XmlFileValuesProviderArgs<TIn, TParsed, TOut> _args;
        public XmlFileValuesProvider(XmlFileValuesProviderArgs<TIn, TParsed, TOut> args)
        {
            _args = args;
        }
        public void PushValues(TIn input, Action<TOut> push)
        {
            using (var s = _args.DataStreamSelector(input))
            {
                var xmlFileDefinition = new XmlFileDefinition()
                    .AddNodeDefinition(_args.XmlNodeDefinition)
                    .AddNameSpaces(_args.PrefixToUriNamespaces);
                XmlObjectReader xmlObjectReader = new XmlObjectReader(xmlFileDefinition);
                xmlObjectReader.Read(s, i => push(_args.ResultSelector(input, i.GetValue<TParsed>())));
            }
        }
    }
    public class XmlFileValuesProvider<TIn, TOut>
    {
        private XmlFileValuesProviderArgs<TIn, TOut> _args;
        public XmlFileValuesProvider(XmlFileValuesProviderArgs<TIn, TOut> args)
        {
            _args = args;
        }
        public void PushValues(TIn input, Action<TOut> push)
        {
            using (var s = _args.DataStreamSelector(input))
            {
                XmlObjectReader xmlObjectReader = new XmlObjectReader(_args.XmlFileDefinition);
                xmlObjectReader.Read(s, i => push(_args.ResultSelector(input, i)));
            }
        }
    }
}