using System;
using System.Threading;
using Paillave.Etl.Core;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;
using System.Text.Json;

namespace Paillave.Etl.S3;

public class S3FileValueProvider(string code, string name, string connectionName, S3AdapterConnectionParameters connectionParameters, S3AdapterProviderParameters providerParameters) : FileValueProviderBase<S3AdapterConnectionParameters, S3AdapterProviderParameters>(code, name, connectionName, connectionParameters, providerParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    private class FileSpecificData
    {
        public required string Folder { get; set; }
        public required string FileName { get; set; }
    }
    public override IFileValue Provide(string fileSpecific)
    {
        var fileSpecificData = JsonSerializer.Deserialize<FileSpecificData>(fileSpecific) ?? throw new Exception("Invalid file specific");
        return new S3FileValue(connectionParameters, fileSpecificData.Folder, fileSpecificData.FileName);
    }
    protected override void Provide(object input, Action<IFileValue, FileReference> pushFileValue, S3AdapterConnectionParameters connectionParameters, S3AdapterProviderParameters providerParameters, CancellationToken cancellationToken)
    {
        var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        var matcher = new Matcher().AddInclude(searchPattern);
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder)
            ? (providerParameters.SubFolder ?? "")
            : StringEx.ConcatenatePath(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");

        var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => S3FileValueProvider.GetFileList(connectionParameters, providerParameters));
        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (matcher.Match(file.Name).HasMatches)
            {
                var fileValue = new S3FileValue(connectionParameters, folder, file.Name);
                var fileReference = new FileReference(fileValue.Name, this.Code, JsonSerializer.Serialize(new FileSpecificData { FileName = file.Name, Folder = folder }));
                pushFileValue(fileValue, fileReference);
            }
        }
    }
    private static List<S3FileItem> GetFileList(S3AdapterConnectionParameters connectionParameters, S3AdapterProviderParameters providerParameters)
    {
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : StringEx.ConcatenatePath(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        using var client = connectionParameters.CreateBucketConnection();
        return client.ListAsync(folder, providerParameters.Recursive).Result;
    }
    protected override void Test(S3AdapterConnectionParameters connectionParameters, S3AdapterProviderParameters providerParameters)
    {
        using var client = connectionParameters.CreateBucketConnection();
        client.ListAsync().Wait();
    }
}
