using Paillave.Etl.Core;
using Paillave.Etl.XmlFile.Core;
using System;
using System.Threading;

namespace Paillave.Etl.XmlFile
{
    public class XmlFileValuesProviderArgs
    {
        public XmlFileDefinition XmlFileDefinition { get; set; }
        public bool UseStreamCopy { get; set; } = true;
    }
    public class XmlFileValuesProvider : ValuesProviderBase<IFileValue, XmlNodeParsed>
    {
        private XmlFileValuesProviderArgs _args;
        public XmlFileValuesProvider(XmlFileValuesProviderArgs args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public override void PushValues(IFileValue input, Action<XmlNodeParsed> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            using var stream = input.Get(_args.UseStreamCopy);
            IXmlObjectReader xmlObjectReader = new XmlObjectReaderV2(_args.XmlFileDefinition, input.Name, push);
            xmlObjectReader.Read(stream, cancellationToken);
        }
    }
}