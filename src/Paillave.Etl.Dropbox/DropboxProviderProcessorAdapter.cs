using System.ComponentModel.DataAnnotations;
using Paillave.Etl.Core;

namespace Paillave.Etl.Dropbox
{
    public class DropboxAdapterConnectionParameters : IDropboxConnectionInfo
    {
        [Required]
        [Sensitive]
        public string Token { get; set; }
        [Sensitive]
        public string AppKey { get; set; }
        [Sensitive]
        public string AppSecret { get; set; }
        public int MaxAttempts { get; set; } = 3;
        public string RootFolder { get; set; }
    }
    public class DropboxAdapterProviderParameters
    {
        public string SubFolder { get; set; }
        public string FileNamePattern { get; set; }
    }
    public class DropboxAdapterProcessorParameters
    {
        public string SubFolder { get; set; }
        public bool UseStreamCopy { get; set; } = true;
    }
    public class DropboxProviderProcessorAdapter : ProviderProcessorAdapterBase<DropboxAdapterConnectionParameters, DropboxAdapterProviderParameters, DropboxAdapterProcessorParameters>
    {
        public override string Description => "Get and save files on an SFTP server";
        public override string Name => "Dropbox";
        protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProviderParameters inputParameters)
            => new DropboxFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);
        protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, DropboxAdapterConnectionParameters connectionParameters, DropboxAdapterProcessorParameters outputParameters)
            => new DropboxFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
    }
}