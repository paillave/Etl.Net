using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Linq;
using System.Collections.Generic;
using Amazon.S3.Model;

namespace Paillave.Etl.S3;
public class S3FileValueProvider : FileValueProviderBase<S3AdapterConnectionParameters, S3AdapterProviderParameters>
{
    public S3FileValueProvider(string code, string name, string connectionName, S3AdapterConnectionParameters connectionParameters, S3AdapterProviderParameters providerParameters)
        : base(code, name, connectionName, connectionParameters, providerParameters) { }
    public S3FileValueProvider(string code, string name, string serviceUrl, string bucket, string accessKeyId, string accessKeySecret, string? fileNamePattern = null)
        : base(code, name, name, new S3AdapterConnectionParameters
        {
            AccessKeyId = accessKeyId,
            AccessKeySecret = accessKeySecret,
            Bucket = bucket,
            ServiceUrl = serviceUrl
        }, new S3AdapterProviderParameters
        {
            FileNamePattern = fileNamePattern
        })
    { }
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    protected override void Provide(Action<IFileValue> pushFileValue, S3AdapterConnectionParameters connectionParameters, S3AdapterProviderParameters providerParameters, CancellationToken cancellationToken, IExecutionContext context)
    {
        var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        var matcher = new Matcher().AddInclude(searchPattern);
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");

        var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => GetFileList(connectionParameters, providerParameters));
        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (matcher.Match(file.Name).HasMatches)
                pushFileValue(new S3FileValue(connectionParameters, folder, file.Name, this.Code, this.Name, this.ConnectionName));
        }
    }
    private class S3FileItem
    {
        public string? FolderName { get; set; }
        public string Name { get; set; }
        public S3Object S3Object { get; set; }
    }
    private List<S3FileItem> GetFileList(S3AdapterConnectionParameters connectionParameters, S3AdapterProviderParameters providerParameters)
    {
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        using (var client = connectionParameters.CreateBucketConnection())
        {
            // folder
            var items = client.ListAsync().Result;
            return items
                .Select(i =>
                {
                    if (i.Key.EndsWith('/'))
                    {
                        return null;
                    }
                    var parent = Path.GetDirectoryName(i.Key);
                    string? fileName = Path.GetFileName(i.Key);
                    return new S3FileItem
                    {
                        FolderName = string.IsNullOrWhiteSpace(parent) ? null : parent,
                        Name = fileName,
                        S3Object = i
                    };
                })
                .Where(i => i != null)
                .Where(i =>
                {
                    if (string.IsNullOrWhiteSpace(folder))
                    {
                        if (providerParameters.Recursive ?? false)
                        {
                            return true;
                        }
                        else
                        {
                            return i!.FolderName == null;
                        }
                    }
                    else
                    {
                        if (providerParameters.Recursive ?? false)
                        {
                            return i!.FolderName?.Equals(folder, StringComparison.InvariantCultureIgnoreCase) ?? false;

                        }
                        else
                        {
                            return i!.FolderName?.StartsWith(folder, StringComparison.InvariantCultureIgnoreCase) ?? false;
                        }
                    }
                })
                .ToList()!;
        }
    }
    protected override void Test(S3AdapterConnectionParameters connectionParameters, S3AdapterProviderParameters providerParameters)
    {
        // var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        // var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        // var matcher = new Matcher().AddInclude(searchPattern);
        using (var client = connectionParameters.CreateBucketConnection())
        {
            client.ListAsync().Wait();
        }
    }
}
