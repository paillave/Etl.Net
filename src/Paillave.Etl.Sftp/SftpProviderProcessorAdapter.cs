using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using System.Net;
using Microsoft.Extensions.FileSystemGlobbing;
using Renci.SshNet;

namespace Paillave.Etl.Sftp
{
    public class SftpAdapterConnectionParameters : ISftpConnectionInfo
    {
        public string RootFolder { get; set; }
        [Required]
        public string Server { get; set; }
        public int PortNumber { get; set; } = 22;
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        public string PrivateKeyPassPhrase { get; set; }
        public string PrivateKey { get; set; }
    }
    public class SftpAdapterProviderParameters
    {
        public string SubFolder { get; set; }
        public string FileNamePattern { get; set; }
    }
    public class SftpAdapterProcessorParameters
    {
        public string SubFolder { get; set; }
    }
    public class SftpProviderProcessorAdapter : ProviderProcessorAdapterBase<SftpAdapterConnectionParameters, SftpAdapterProviderParameters, SftpAdapterProcessorParameters>
    {
        public override string Description => "Get and save files on an SFTP server";
        public override string Name => "Sftp";
        private class SftpFileValueProvider : FileValueProviderBase<SftpAdapterConnectionParameters, SftpAdapterProviderParameters>
        {
            public SftpFileValueProvider(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters)
                : base(code, name, connectionName, connectionParameters, providerParameters) { }
            public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
            public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
            protected override void Provide(Action<IFileValue> pushFileValue, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters, CancellationToken cancellationToken, IDependencyResolver resolver)
            {
                var folder = Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
                var searchPattern = string.IsNullOrEmpty(providerParameters.FileNamePattern) ? "*" : providerParameters.FileNamePattern;
                var matcher = new Matcher().AddInclude(searchPattern);
                var connectionInfo = connectionParameters.CreateConnectionInfo();
                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();
                    var files = client.ListDirectory(folder);
                    foreach (var file in files)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        if (matcher.Match(file.Name).HasMatches)
                            pushFileValue(new SftpFileValue(connectionParameters, folder, file.Name, this.Code, this.Name, this.ConnectionName));
                    }
                }
            }

            protected override void Test(SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters providerParameters)
            {
                var folder = Path.Combine(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
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
        private class SftpFileValueProcessor : FileValueProcessorBase<SftpAdapterConnectionParameters, SftpAdapterProcessorParameters>
        {
            public SftpFileValueProcessor(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters processorParameters)
                : base(code, name, connectionName, connectionParameters, processorParameters) { }
            public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
            public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
            protected override void Process(IFileValue fileValue, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken, IDependencyResolver resolver)
            {
                var folder = Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
                var connectionInfo = connectionParameters.CreateConnectionInfo();
                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();
                    var stream = fileValue.GetContent();
                    byte[] fileContents;
                    stream.Position = 0;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        fileContents = ms.ToArray();
                    }

                    client.WriteAllBytes(Path.Combine(folder, fileValue.Name), fileContents);
                }
                push(fileValue);
            }

            protected override void Test(SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters processorParameters)
            {
                var fileName = Guid.NewGuid().ToString();
                var folder = Path.Combine(connectionParameters.RootFolder, processorParameters.SubFolder ?? "");
                var connectionInfo = connectionParameters.CreateConnectionInfo();
                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();
                    var stream = new MemoryStream();
                    byte[] fileContents;
                    stream.Position = 0;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        fileContents = ms.ToArray();
                    }

                    client.WriteAllBytes(Path.Combine(folder, fileName), fileContents);
                }
                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();
                    client.DeleteFile(Path.Combine(folder, fileName));
                }
            }
        }
        protected override IFileValueProvider CreateProvider(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProviderParameters inputParameters)
            => new SftpFileValueProvider(code, name, connectionName, connectionParameters, inputParameters);
        protected override IFileValueProcessor CreateProcessor(string code, string name, string connectionName, SftpAdapterConnectionParameters connectionParameters, SftpAdapterProcessorParameters outputParameters)
            => new SftpFileValueProcessor(code, name, connectionName, connectionParameters, outputParameters);
    }
}