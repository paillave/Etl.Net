using System;
using System.IO;
using System.Threading;
using Paillave.Etl.Core;
using Microsoft.Extensions.FileSystemGlobbing;
using FluentFtp;
using System.Linq;

namespace Paillave.Etl.HttpExtension2
{
    public class HttpFileValueProvider : FileValueProviderBase<HttpAdapterConnectionParameters, HttpAdapterProviderParameters>
    {
        public HttpFileValueProvider(string code, string name, string connectionName, HttpAdapterConnectionParameters connectionParameters, HttpAdapterProviderParameters providerParameters)
            : base(code, name, connectionName, connectionParameters, providerParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Provide(Action<IFileValue> pushFileValue, HttpAdapterConnectionParameters connectionParameters, HttpAdapterProviderParameters providerParameters, CancellationToken cancellationToken, IExecutionContext context)
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
                    pushFileValue(new HttpFileValue(connectionParameters, Path.GetDirectoryName(fullPath), fileName, this.Code, this.Name, this.ConnectionName));
            }
        }
        private HttpListItem[] GetFileList(HttpAdapterConnectionParameters connectionParameters, HttpAdapterProviderParameters providerParameters)
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : StringEx.ConcatenatePath(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
            using (HttpClient client = connectionParameters.CreateHttpClient())
            {
                return (providerParameters.Recursive ? client.GetListing(folder, HttpListOption.Recursive) : client.GetListing(folder)).Where(i => i.Type == HttpObjectType.File).ToArray();
            }
        }
        protected override void Test(HttpAdapterConnectionParameters connectionParameters, HttpAdapterProviderParameters providerParameters)
        {
            var folder = string.IsNullOrWhiteSpace(connectionParameters.RootFolder) ? (providerParameters.SubFolder ?? "") : StringEx.ConcatenatePath(connectionParameters.RootFolder, providerParameters.SubFolder ?? "");
            using (HttpClient client = connectionParameters.CreateHttpClient())
            {
                if (providerParameters.Recursive)
                    client.GetListing(folder, HttpListOption.Recursive);
                else
                    client.GetListing(folder);
            }
        }
    }
}