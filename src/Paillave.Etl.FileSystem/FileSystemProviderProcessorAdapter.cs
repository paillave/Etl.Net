using System.ComponentModel.DataAnnotations;
using Paillave.Etl.Core;

namespace Paillave.Etl.FileSystem
{
    public class FileSystemAdapterConnectionParameters
    {
        [Required]
        public string RootFolder { get; set; }
    }
    public class FileSystemAdapterProviderParameters
    {
        public string SubFolder { get; set; }
        public string FileNamePattern { get; set; }
        public bool Recursive { get; set; }
    }
    public class FileSystemAdapterProcessorParameters
    {
        public string SubFolder { get; set; }
        public bool UseStreamCopy { get; set; } = true;
        public bool BuildMissingSubFolders { get; set; } = false;
    }
    public class FileSystemProviderProcessorAdapter : ProviderProcessorAdapterBase<FileSystemAdapterConnectionParameters, FileSystemAdapterProviderParameters, FileSystemAdapterProcessorParameters>
    {
        public override string Description => "Get and save files on the local file system";
        public override string Name => "FileSystem";
        protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProviderParameters inputParameters)
            => new FileSystemFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);

        protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, FileSystemAdapterConnectionParameters connectionParameters, FileSystemAdapterProcessorParameters outputParameters)
            => new FileSystemFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
    }
}