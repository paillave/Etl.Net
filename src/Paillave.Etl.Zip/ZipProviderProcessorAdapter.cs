using System;
using System.Threading;
using Paillave.Etl.Core;

namespace Paillave.Etl.Zip;

public class ZipAdapterConnectionParameters
{
    public string Password { get; set; }
}
public enum ZipDirection
{
    Unzip,
    Zip
}
public class ZipAdapterProcessorParameters
{
    public ZipDirection Direction { get; set; } = ZipDirection.Unzip;
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
    protected override void Process(IFileValue fileValue, ZipAdapterConnectionParameters connectionParameters, ZipAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken)
    {
        switch (processorParameters.Direction)
        {
            case ZipDirection.Unzip:
                new UnzipFileProcessorValuesProvider(new UnzipFileProcessorParams
                {
                    FileNamePattern = processorParameters.FileNamePattern,
                    Password = connectionParameters.Password
                }).PushValues(fileValue, push, cancellationToken);
                break;
            case ZipDirection.Zip:
                new ZipFileProcessorValuesProvider(new ZipFileProcessorParams
                {
                    Password = connectionParameters.Password
                }).PushValues(fileValue, push, cancellationToken);
                break;
        }
    }

    protected override void Test(ZipAdapterConnectionParameters connectionParameters, ZipAdapterProcessorParameters processorParameters) { }
}
