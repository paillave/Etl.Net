using Paillave.Etl.Core;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.XmlFile.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace Paillave.Etl.XmlFile
{
    public class XmlFileValuesProviderArgs
    {
        public XmlFileDefinition XmlFileDefinition { get; set; }
    }
    public class XmlFileValuesProvider : ValuesProviderBase<IFileValue, XmlNodeParsed>
    {
        private XmlFileValuesProviderArgs _args;
        public XmlFileValuesProvider(XmlFileValuesProviderArgs args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public override void PushValues(IFileValue input, Action<XmlNodeParsed> push, CancellationToken cancellationToken, IDependencyResolver resolver)
        {
            using (var s = input.GetContent())
            {
                XmlObjectReader xmlObjectReader = new XmlObjectReader(_args.XmlFileDefinition);
                xmlObjectReader.Read(s, input.Name, push, cancellationToken);
            }
        }
    }
}