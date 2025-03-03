using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;

namespace Paillave.Etl.S3;
public class S3FileValueProcessor : FileValueProcessorBase<S3AdapterConnectionParameters, S3AdapterProcessorParameters>
{
    public S3FileValueProcessor(string code, string name, string connectionName, S3AdapterConnectionParameters connectionParameters, S3AdapterProcessorParameters processorParameters)
        : base(code, name, connectionName, connectionParameters, processorParameters) { }
    public S3FileValueProcessor(string code, string name, string serviceUrl, string bucket, string accessKeyId, string accessKeySecret)
        : base(code, name, name, new S3AdapterConnectionParameters
        {
            AccessKeyId = accessKeyId,
            AccessKeySecret = accessKeySecret,
            Bucket = bucket,
            ServiceUrl = serviceUrl
        },
        new S3AdapterProcessorParameters { })
    { }
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    protected override void Process(IFileValue fileValue, S3AdapterConnectionParameters connectionParameters, S3AdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
    {
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (processorParameters.SubFolder ?? "") : StringEx.ConcatenatePath(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
        using var stream = fileValue.Get(processorParameters.UseStreamCopy);
        ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => UploadSingleTime(connectionParameters, stream, StringEx.ConcatenatePath(folder, fileValue.Name)));
        push(fileValue);
    }
    private void UploadSingleTime(S3AdapterConnectionParameters connectionParameters, Stream stream, string filePath)
    {
        using (var client = connectionParameters.CreateBucketConnection())
        {
            client.UploadAsync(filePath, stream).Wait();
        }
    }
    protected override void Test(S3AdapterConnectionParameters connectionParameters, S3AdapterProcessorParameters processorParameters)
    {
        var fileName = Guid.NewGuid().ToString();
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (processorParameters.SubFolder ?? "") : StringEx.ConcatenatePath(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
        using (var client = connectionParameters.CreateBucketConnection())
        {
            client.UploadAsync(StringEx.ConcatenatePath(folder, fileName), new MemoryStream()).Wait();
        }
        using (var client = connectionParameters.CreateBucketConnection())
        {
            client.DeleteAsync(StringEx.ConcatenatePath(folder, fileName)).Wait();
        }
    }
}
