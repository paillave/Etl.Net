using System;
using System.Threading;
using Paillave.Etl.Core;
using Paillave.Etl.Connector;
using Paillave.Etl.ValuesProviders;
using MailKit.Search;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;

namespace Paillave.Etl.Mail
{
    public class MailAdapterProviderParameters
    {
        public bool? OnlyNotRead { get; set; }
        public string FromContains { get; set; }
        public string ToContains { get; set; }
        public string SubjectContains { get; set; }
        public string AttachmentNamePattern { get; set; }
        public string Folder { get; set; }
        public bool SetToReadIfBatchDeletion { get; set; }
    }
    public class MailFileValueProvider : FileValueProviderBase<MailAdapterConnectionParameters, MailAdapterProviderParameters>
    {
        public MailFileValueProvider(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProviderParameters providerParameters)
            : base(code, name, connectionName, connectionParameters, providerParameters) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override void Provide(Action<IFileValue> pushFileValue, MailAdapterConnectionParameters connectionParameters, MailAdapterProviderParameters providerParameters, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => GetFileList(connectionParameters, providerParameters));
            foreach (var item in files)
            {
                if (cancellationToken.IsCancellationRequested) break;
                pushFileValue(item);
            }
        }
        private List<MailFileValue> GetFileList(MailAdapterConnectionParameters connectionParameters, MailAdapterProviderParameters providerParameters)
        {
            List<MailFileValue> fileValues = new List<MailFileValue>();
            using (var client = connectionParameters.CreateIMapClient())
            {
                var folder = string.IsNullOrWhiteSpace(providerParameters.Folder)
                    ? client.Inbox
                    : client.GetFolder(providerParameters.Folder);
                var query = CreateQuery(providerParameters);
                var searchResult = folder.Search(query);
                var matcher = string.IsNullOrWhiteSpace(providerParameters.AttachmentNamePattern) ? null : new Matcher().AddInclude(providerParameters.AttachmentNamePattern);
                foreach (var item in searchResult)
                {
                    var message = folder.GetMessage(item);
                    var attachments = message.Attachments.ToList();
                    var deletionDico = attachments.ToDictionary(i => i.ContentDisposition.FileName, i => false);
                    foreach (var attachment in message.Attachments)
                    {
                        if (matcher == null || matcher.Match(attachment.ContentDisposition.FileName).HasMatches)
                        {
                            var fileValue = new MailFileValue(
                                connectionParameters,
                                providerParameters.Folder,
                                attachment.ContentDisposition.FileName,
                                this.Code,
                                this.Name,
                                this.ConnectionName,
                                providerParameters.SetToReadIfBatchDeletion,
                                message.Subject,
                                message.Date.DateTime,
                                message.From.First().Name,
                                item.Id,
                                deletionDico
                            );
                            fileValues.Add(fileValue);
                        }
                    }
                }
            }
            return fileValues;
        }
        private SearchQuery CreateQuery(MailAdapterProviderParameters providerParameters)
        {
            SearchQuery query = SearchQuery.All;
            if (!string.IsNullOrWhiteSpace(providerParameters.FromContains))
            {
                query = query.And(SearchQuery.FromContains(providerParameters.FromContains.Trim()));
            }
            if (!string.IsNullOrWhiteSpace(providerParameters.ToContains))
            {
                query = query.And(SearchQuery.ToContains(providerParameters.ToContains.Trim()));
            }
            if (!string.IsNullOrWhiteSpace(providerParameters.SubjectContains))
            {
                query = query.And(SearchQuery.SubjectContains(providerParameters.SubjectContains.Trim()));
            }
            if (providerParameters.OnlyNotRead.HasValue && providerParameters.OnlyNotRead.Value)
            {
                query = query.And(SearchQuery.NotSeen);
            }
            return query;
        }
        protected override void Test(MailAdapterConnectionParameters connectionParameters, MailAdapterProviderParameters providerParameters)
        {
            using (connectionParameters.CreateIMapClient()) ;
        }
    }
}