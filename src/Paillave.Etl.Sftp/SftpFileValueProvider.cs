using System;
using System.Threading;
using Paillave.Etl.Core;
using Microsoft.Extensions.FileSystemGlobbing;
using Renci.SshNet;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Paillave.Etl.Sftp;

public class SftpFileValueProvider(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters) : FileValueProviderBase<SftpAdapterConnectionParameters, SftpAdapterProviderParameters>(code, name, connectionName, connectionParameters, providerParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    private class FileSpecificData
    {
        public required string Folder { get; set; }
        public required string FileName { get; set; }
    }
    public override IFileValue Provide(JsonNode? fileSpecific)
    {
        var fileSpecificData = JsonSerializer.Deserialize<FileSpecificData>(fileSpecific) ?? throw new Exception("Invalid file specific");
        return new SftpFileValue(connectionParameters, fileSpecificData.Folder, fileSpecificData.FileName);
    }
    protected override void Provide(object input, Action<IFileValue, FileReference> pushFileValue, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters, CancellationToken cancellationToken)
    {
        var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        var matcher = new Matcher().AddInclude(searchPattern);
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : ConcatenatePath(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");

        var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => GetFileList(connectionParameters, providerParameters));
        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested) break;
            if (matcher.Match(file.Name).HasMatches)
            {
                var fileValue = new SftpFileValue(connectionParameters, folder, file.Name);
                var fileReference = new FileReference(fileValue.Name, this.Code,  JsonSerializer.SerializeToNode(new FileSpecificData { FileName = file.Name, Folder = folder }));
                pushFileValue(fileValue, fileReference);
            }
        }
    }
    private static string ConcatenatePath(params string[] segments)
    {
        return string.Join("/", segments.Where(i => !string.IsNullOrWhiteSpace(i))).Replace("//", "/");
    }
    private Renci.SshNet.Sftp.ISftpFile[] GetFileList(SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters)
    {
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder)
            ? (providerParameters.SubFolder ?? "")
            : ConcatenatePath(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        var connectionInfo = connectionParameters.CreateConnectionInfo();
        using (var client = new SftpClient(connectionInfo))
        {
            client.Connect();
            return client.ListDirectory(folder).Where(i => !i.IsDirectory).ToArray();
        }
    }
    protected override void Test(SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters)
    {
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : ConcatenatePath(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        var matcher = new Matcher().AddInclude(searchPattern);
        var connectionInfo = connectionParameters.CreateConnectionInfo();
        using (var client = new SftpClient(connectionInfo))
        {
            client.Connect();
            client.ListDirectory(folder);
        }
    }
}
