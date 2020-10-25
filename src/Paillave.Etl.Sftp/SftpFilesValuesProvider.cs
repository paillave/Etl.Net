using Microsoft.Extensions.FileSystemGlobbing;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Renci.SshNet;
using System.Text;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.Core.Streams;
using System.Threading;

namespace Paillave.Etl.Sftp
{

    public class SftpFilesValuesProviderArgs<TIn, TOut>
    {
        public Func<TIn, string> GetFolderPath { get; set; }
        public IStream<TIn> Stream { get; set; }
        public SftpConnectionInfo ConnectionInfo { get; set; }
        public Func<IFileValue, TIn, TOut> GetResult { get; set; }
        public Func<TIn, string> GetSearchPattern { get; set; }
    }

    public class SftpFilesValuesProvider<TIn, TOut> : ValuesProviderBase<TIn, TOut>
    {
        private readonly SftpFilesValuesProviderArgs<TIn, TOut> _args;

        public SftpFilesValuesProvider(SftpFilesValuesProviderArgs<TIn, TOut> args)
        {
            this._args = args;
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver)
        {
            var sftpConnectionInfo = _args.ConnectionInfo ?? resolver.Resolve<SftpConnectionInfo>();
            var connectionInfo = sftpConnectionInfo.CreateConnectionInfo();
            string path = this._args?.GetFolderPath(input);
            var matcher = _args.GetSearchPattern == null ? new Matcher().AddInclude("*") : new Matcher().AddInclude(_args.GetSearchPattern(input) ?? "*");
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                var files = client.ListDirectory(path);
                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    if (matcher.Match(file.Name).HasMatches)
                        push(_args.GetResult(new SftpFileValue(sftpConnectionInfo, path, file.Name), input));
                }
            }
        }
    }
}
