using Microsoft.Graph.Models;
using Paillave.Etl.Core;
using Paillave.Etl.GraphApi.Provider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Paillave.Etl.GraphApi;

public class GraphApiMailFileValue : FileValueBase
{
    public override string Name { get; }
    public string Subject { get; }
    public string SenderAddress { get; }
    public string SenderName { get; }
    public DateTime? ReceivedDateTime { get; }
    private readonly string _userId;
    private readonly string _messageId;
    private readonly string _attachmentId;
    private readonly bool _setToReadIfBatchDeletion;
    private readonly IGraphApiConnectionInfo _connectionInfo;
    private readonly Dictionary<string, bool> _deleted;
    internal GraphApiMailFileValue(
        IGraphApiConnectionInfo connectionInfo, string messageId, string attachmentId, string attachmentName,
        bool setToReadIfBatchDeletion, Dictionary<string, bool> deleted, string subject, string senderAddress, string senderName, DateTime? receivedDateTime)
            => (Name, _userId, _messageId, _attachmentId, _connectionInfo, _deleted, _setToReadIfBatchDeletion, Subject, SenderAddress, SenderName, ReceivedDateTime)
            = (attachmentName, connectionInfo.UserId, messageId, attachmentId, connectionInfo, deleted, setToReadIfBatchDeletion, subject, senderAddress, senderName, receivedDateTime);
    protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
    protected void DeleteFileSingleTime()
    {
        _deleted[this._attachmentId] = true;
        if (_deleted.Values.All(v => v))
        {
            using var client = _connectionInfo.CreateGraphApiClient();
            var messageRequestBuilder = client.Users[_userId].Messages[_messageId];
            if (_setToReadIfBatchDeletion)
            {
                var messageToDelete = messageRequestBuilder.GetAsync(i =>
                {
                    i.QueryParameters.Select = ProjectionProcessor<Message>.ToString(m => new { m.IsRead, m.Id });
                }).GetAwaiter().GetResult() ?? throw new System.Exception("Message not found");
                messageToDelete.IsRead = true;
                messageRequestBuilder.PatchAsync(messageToDelete);
            }
            else
            {
                messageRequestBuilder.DeleteAsync().GetAwaiter().GetResult();
            }
        }
    }
    public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
    private Stream GetContentSingleTime()
    {
        using var graphClient = _connectionInfo.CreateGraphApiClient();
        var fullMessageRequestBuilder = graphClient.Users[_userId].Messages[_messageId];

        var attachment = (FileAttachment)(fullMessageRequestBuilder.Attachments[_attachmentId].GetAsync().GetAwaiter().GetResult()
            ?? throw new System.Exception("Attachment not found"));
        var ms = new MemoryStream();
        ms.Write(attachment?.ContentBytes ?? []);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public override StreamWithResource OpenContent() => new StreamWithResource(GetContent());
}
