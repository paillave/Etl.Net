using System;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;

namespace Paillave.Etl.FileSystem;

public class FileSystemFileValueProvider : FileValueProviderBase<FileSystemAdapterConnectionParameters, FileSystemAdapterProviderParameters>
{
    public FileSystemFileValueProvider(string code, string name, string rootFolder, string fileNamePattern)
        : base(code, name, name, new FileSystemAdapterConnectionParameters
        {
            RootFolder = rootFolder
        }, new FileSystemAdapterProviderParameters
        {
            FileNamePattern = fileNamePattern
        })
    { }
    public FileSystemFileValueProvider(string code, string name, string connectionName, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProviderParameters providerParameters)
        : base(code, name, connectionName, connectionParameters, providerParameters) { }
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    public override IFileValue Provide(string fileSpecific)
        => new FileSystemFileValue(new FileInfo(fileSpecific));
    protected override void Provide(object input, Action<IFileValue, FileReference> pushFileValue, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProviderParameters providerParameters, CancellationToken cancellationToken)
    {
        var rootFolder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        var files = Directory
            .GetFiles(rootFolder, searchPattern, providerParameters.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .ToList();
        var isRootedPath = Path.IsPathRooted(rootFolder);
        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested) break;
            var filePath = isRootedPath ? Path.Combine(rootFolder, file) : file;
            var fileValue = new FileSystemFileValue(new FileInfo(filePath));
            var fileReference = new FileReference(fileValue.Name, this.Code, filePath);
            pushFileValue(fileValue, fileReference);
        }
    }
    protected override void Test(FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProviderParameters providerParameters)
    {
        var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        // var folder = Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
        var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
        var files = Directory
            .GetFiles(folder, searchPattern, providerParameters.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }
}