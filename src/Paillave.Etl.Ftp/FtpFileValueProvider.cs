using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Microsoft.Extensions.FileSystemGlobbing;
using FluentFTP;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Paillave.Etl.Ftp;

public class FtpFileValueProvider(string code, string name, string connectionName, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters) : FileValueProviderBase<FtpAdapterConnectionParameters, FtpAdapterProviderParameters>(code, name, connectionName, connectionParameters, providerParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    protected override void Provide(object input, Action<IFileValue, FileReference> pushFileValue, FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters, CancellationToken cancellationToken)
    {
        var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        var matcher = new Matcher().AddInclude(searchPattern);
        var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => GetFileList(connectionParameters, providerParameters));
        foreach (var item in files)
        {
            if (cancellationToken.IsCancellationRequested) break;
            var fullPath = item.FullName;
            var fileName = Path.GetFileName(fullPath);
            fileName = Path.GetFileName(fileName);
            if (matcher.Match(fileName).HasMatches)
            {
                var folder = Path.GetDirectoryName(fullPath);
                var fileValue = new FtpFileValue(connectionParameters, folder, fileName);
                var fileReference = new FileReference(fileValue.Name, this.Code, JsonSerializer.SerializeToNode(new FileSpecificData { FileName = fileName, Folder = folder }));
                pushFileValue(fileValue, fileReference);
            }
        }
    }
    public override IFileValue Provide(JsonNode? fileSpecific)
    {
        var fileSpecificData = JsonSerializer.Deserialize<FileSpecificData>(fileSpecific) ?? throw new Exception("Invalid file specific");
        return new FtpFileValue(connectionParameters, fileSpecificData.Folder, fileSpecificData.FileName);
    }

    private class FileSpecificData
    {
        public required string? Folder { get; set; }
        public required string FileName { get; set; }
    }
    private FtpListItem[] GetFileList(FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters)
    {
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : StringEx.ConcatenatePath(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        using (FtpClient client = connectionParameters.CreateFtpClient())
        {
            return (providerParameters.Recursive ? client.GetListing(folder, FtpListOption.Recursive) : client.GetListing(folder)).Where(i => i.Type == FtpObjectType.File).ToArray();
        }
    }
    protected override void Test(FtpAdapterConnectionParameters connectionParameters, FtpAdapterProviderParameters providerParameters)
    {
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : StringEx.ConcatenatePath(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        using (FtpClient client = connectionParameters.CreateFtpClient())
        {
            if (providerParameters.Recursive)
                client.GetListing(folder, FtpListOption.Recursive);
            else
                client.GetListing(folder);
        }
    }

}