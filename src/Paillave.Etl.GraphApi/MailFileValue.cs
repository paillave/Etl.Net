using Microsoft.Graph.Models;
using Paillave.Etl.Core;
using Paillave.Etl.GraphApi.Provider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Paillave.Etl.GraphApi;

public class MailFileValue : FileValueBase<GraphApiMailFileValueMetadata>
{
    public override string Name { get; }
    private readonly string _userId;
    private readonly string _messageId;
    private readonly string _attachmentId;
    private bool _setToReadIfBatchDeletion;
    private readonly IGraphApiConnectionInfo _connectionInfo;
    private readonly Dictionary<string, bool> _deleted;
    internal MailFileValue(
        IGraphApiConnectionInfo connectionInfo,
        string messageId, string attachmentId,
        string folder, string mailSubject, string attachmentName,
        string sender, DateTime receivedDate,
        string connectorCode, string connectorName, string connectionName,
        bool setToReadIfBatchDeletion, Dictionary<string, bool> deleted)
        : base(new GraphApiMailFileValueMetadata
        {
            ClientId = connectionInfo.ClientId,
            TenantId = connectionInfo.TenantId,

            UserId = connectionInfo.UserId,
            MessageId = messageId,
            AttachmentId = attachmentId,

            Folder = folder,
            MailSubject = mailSubject,
            Name = attachmentName,

            Sender = sender,
            ReceivedDate = receivedDate,

            ConnectorCode = connectorCode,
            ConnectionName = connectionName,
            ConnectorName = connectorName,
        }) => (Name, _userId, _messageId, _attachmentId, _connectionInfo, _deleted, _setToReadIfBatchDeletion)
            = (attachmentName, connectionInfo.UserId, messageId, attachmentId, connectionInfo, deleted, setToReadIfBatchDeletion);
    protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
    protected void DeleteFileSingleTime()
    {
        _deleted[this.Name] = true;
        if (_deleted.Values.All(v => v))
        {
            using (var client = _connectionInfo.CreateGraphApiClient())
            {
                var messageRequestBuilder = client.Users[_userId].Messages[_messageId];
                if (_setToReadIfBatchDeletion)
                {
                    var messageToDelete = messageRequestBuilder.GetAsync(i =>
                    {
                        i.QueryParameters.Select = ProjectionProcessor<Message>.ToString(m => new { m.IsRead, m.Id });
                    }).Result;
                    messageToDelete.IsRead = true;
                    messageRequestBuilder.PatchAsync(messageToDelete);
                }
                else
                {
                    messageRequestBuilder.DeleteAsync().Wait();
                }
            }
        }
    }
    public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
    private Stream GetContentSingleTime()
    {
        using (var graphClient = _connectionInfo.CreateGraphApiClient())
        {
            var fullMessageRequestBuilder = graphClient.Users[_userId].Messages[_messageId];

            var attachment = (FileAttachment)fullMessageRequestBuilder.Attachments[_attachmentId].GetAsync().Result;
            using var ms = new MemoryStream();
            ms.Write(attachment?.ContentBytes ?? new byte[] { });
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }

    public override StreamWithResource OpenContent() => new StreamWithResource(GetContent());
}
public class GraphApiMailFileValueMetadata : FileValueMetadataBase
{
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string UserId { get; set; }
    public string MessageId { get; set; }
    public string AttachmentId { get; set; }
    public string Folder { get; set; }
    public string Name { get; set; }
    public string MailSubject { get; set; }
    public DateTime ReceivedDate { get; set; }
    public string Sender { get; set; }
}
