using System;
using System.Threading;
using Paillave.Etl.Core;
using MailKit.Search;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;
using System.Text.Json;

namespace Paillave.Etl.Mail;

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
public partial class MailFileValueProvider(string code, string name, string connectionName, MailAdapterConnectionParameters connectionParameters, MailAdapterProviderParameters providerParameters) : FileValueProviderBase<MailAdapterConnectionParameters, MailAdapterProviderParameters>(code, name, connectionName, connectionParameters, providerParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    private class FileSpecificData
    {
        public required uint MessageId { get; set; }
        public required string Subject { get; set; }
        public required string AttachmentName { get; set; }
        public required DateTime ReceivedDateTime { get; set; }
        public required Dictionary<string, bool> DeletionDico { get; set; }
    }
    public override IFileValue Provide(string fileSpecific)
    {
        var fileSpecificData = JsonSerializer.Deserialize<FileSpecificData>(fileSpecific) ?? throw new Exception("Invalid file specific");
        return new MailFileValue(
            connectionParameters,
            providerParameters.Folder,
            fileSpecificData.AttachmentName,
            providerParameters.SetToReadIfBatchDeletion,
            fileSpecificData.MessageId,
            fileSpecificData.DeletionDico
        );
    }
    protected override void Provide(object input, Action<IFileValue, FileReference> pushFileValue, MailAdapterConnectionParameters connectionParameters, MailAdapterProviderParameters providerParameters, CancellationToken cancellationToken)
    {
        var files = ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => GetFileList(connectionParameters, providerParameters));
        foreach (var (fileValue, fileReference) in files)
        {
            if (cancellationToken.IsCancellationRequested) break;
            pushFileValue(fileValue, fileReference);
        }
    }
    private List<(MailFileValue fileValue, FileReference fileReference)> GetFileList(MailAdapterConnectionParameters connectionParameters, MailAdapterProviderParameters providerParameters)
    {
        var fileValues = new List<(MailFileValue fileValue, FileReference fileReference)>();
        using (var client = connectionParameters.CreateIMapClient())
        {
            var folder = string.IsNullOrWhiteSpace(providerParameters.Folder)
                ? client.Inbox
                : client.GetFolder(providerParameters.Folder);
            var query = MailFileValueProvider.CreateQuery(providerParameters);
            using (var openedFolder = new OpenedMailFolder(folder))
            {
                var searchResult = openedFolder.Search(query);
                var matcher = string.IsNullOrWhiteSpace(providerParameters.AttachmentNamePattern) ? null : new Matcher().AddInclude(providerParameters.AttachmentNamePattern);
                foreach (var item in searchResult)
                {
                    var message = openedFolder.GetMessage(item);
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
                                providerParameters.SetToReadIfBatchDeletion,
                                item.Id,
                                deletionDico
                            );
                            var fileReference = new FileReference(fileValue.Name, this.Code, JsonSerializer.Serialize(new FileSpecificData
                            {
                                MessageId = item.Id,
                                Subject = message.Subject,
                                AttachmentName = attachment.ContentDisposition.FileName,
                                ReceivedDateTime = message.Date.DateTime,
                                DeletionDico = deletionDico,
                            }));
                            fileValues.Add((fileValue, fileReference));
                        }
                    }
                }
            }
        }
        return fileValues;
    }
    private static SearchQuery CreateQuery(MailAdapterProviderParameters providerParameters)
    {
        SearchQuery query = SearchQuery.All;
        if (!string.IsNullOrWhiteSpace(providerParameters.FromContains))
            query = query.And(SearchQuery.FromContains(providerParameters.FromContains.Trim()));
        if (!string.IsNullOrWhiteSpace(providerParameters.ToContains))
            query = query.And(SearchQuery.ToContains(providerParameters.ToContains.Trim()));
        if (!string.IsNullOrWhiteSpace(providerParameters.SubjectContains))
            query = query.And(SearchQuery.SubjectContains(providerParameters.SubjectContains.Trim()));
        if (providerParameters.OnlyNotRead.HasValue && providerParameters.OnlyNotRead.Value)
            query = query.And(SearchQuery.NotSeen);
        return query;
    }
    protected override void Test(MailAdapterConnectionParameters connectionParameters, MailAdapterProviderParameters providerParameters)
    {
        using (connectionParameters.CreateIMapClient()) { }
    }
}