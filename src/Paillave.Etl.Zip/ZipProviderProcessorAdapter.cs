using System;
using System.Threading;
using Paillave.Etl.Core;

namespace Paillave.Etl.Zip
{
    public class ZipAdapterConnectionParameters
    {
        public string Password { get; set; }
    }
    public class ZipAdapterProcessorParameters
    {
        public string FileNamePattern { get; set; }
    }
    public class ZipProviderProcessorAdapter : ProviderProcessorAdapterBase<ZipAdapterConnectionParameters, object, ZipAdapterProcessorParameters>
    {
        public override string Description => "Handle zip files";
        public override string Name => "Zip";
        protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, ZipAdapterConnectionParameters connectionParameters, object inputParameters)
            => null;
        protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, ZipAdapterConnectionParameters connectionParameters, ZipAdapterProcessorParameters outputParameters)
            => new ZipFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
    }
    public class ZipFileValueProcessor : FileValueProcessorBase<ZipAdapterConnectionParameters, ZipAdapterProcessorParameters>
    {
        public ZipFileValueProcessor(string code, string name, string connectionName, ZipAdapterConnectionParameters connectionParameters, ZipAdapterProcessorParameters processorParameters)
            : base(code, name, connectionName, connectionParameters, processorParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Process(IFileValue fileValue, ZipAdapterConnectionParameters connectionParameters, ZipAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            new UnzipFileProcessorValuesProvider(new UnzipFileProcessorParams
            {
                FileNamePattern = processorParameters.FileNamePattern,
                Password = connectionParameters.Password
            }).PushValues(fileValue, push, cancellationToken, context);
        }

        protected override void Test(ZipAdapterConnectionParameters connectionParameters, ZipAdapterProcessorParameters processorParameters) { }
    }
}