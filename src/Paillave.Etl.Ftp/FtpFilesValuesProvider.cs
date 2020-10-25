using Microsoft.Extensions.FileSystemGlobbing;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.ValuesProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Paillave.Etl.Ftp
{

    public class FtpFilesValuesProviderArgs<TIn, TOut>
    {
        public Func<TIn, string> GetFolderPath { get; set; }
        public IStream<TIn> Stream { get; set; }
        public FtpConnectionInfo ConnectionInfo { get; set; }
        public Func<IFileValue, TIn, TOut> GetResult { get; set; }
        public Func<TIn, string> GetSearchPattern { get; set; }
    }

    public class FtpFilesValuesProvider<TIn, TOut> : ValuesProviderBase<TIn, TOut>
    {
        private readonly FtpFilesValuesProviderArgs<TIn, TOut> _args;
        public FtpFilesValuesProvider(FtpFilesValuesProviderArgs<TIn, TOut> args)
        {
            this._args = args;
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver)
        {
            var connectionInfo = _args.ConnectionInfo ?? resolver.Resolve<FtpConnectionInfo>();
            string path = this._args?.GetFolderPath(input);
            // var connectionInfo = ftpConnectionInfo.CreateConnectionInfo();
            UriBuilder uriBuilder = new UriBuilder("ftp", connectionInfo.Server, connectionInfo.PortNumber, path);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uriBuilder.Uri);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            request.Credentials = new NetworkCredential(connectionInfo.Login, connectionInfo.Password);
            var matcher = _args.GetSearchPattern == null ? new Matcher().AddInclude("*") : new Matcher().AddInclude(_args.GetSearchPattern(input) ?? "*");

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                while (!reader.EndOfStream)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    var fileName = reader.ReadLine();
                    if (matcher.Match(fileName).HasMatches)
                        push(_args.GetResult(new FtpFileValue(connectionInfo, path, fileName), input));
                }
        }
    }
}
