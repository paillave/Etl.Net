using MailKit;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Paillave.Etl.Mail;

public class MailFileValue : FileValueBase
{
    public override string Name { get; }
    public string Subject { get; }
    private readonly uint _messageId;
    private readonly string _folder;
    private bool _setToReadIfBatchDeletion;
    private readonly IMailConnectionInfo _connectionInfo;
    private readonly Dictionary<string, bool> _deleted;
    // public IMapFileValue(IMailConnectionInfo connectionInfo, string folder, string fileName)
    //     : this(connectionInfo, folder, fileName, null, null, null, false) { }
    public MailFileValue(IMailConnectionInfo connectionInfo, string folder, string fileName, bool setToReadIfBatchDeletion, uint messageId, Dictionary<string, bool> deleted, string subject)
        => (Name, _messageId, _connectionInfo, _deleted, _folder, _setToReadIfBatchDeletion, Subject)
        = (fileName, messageId, connectionInfo, deleted, folder, setToReadIfBatchDeletion, subject);
    protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
    protected void DeleteFileSingleTime()
    {
        _deleted[this.Name] = true;
        if (_deleted.Values.All(v => v))
        {
            using var client = _connectionInfo.CreateIMapClient();
            var folder = string.IsNullOrWhiteSpace(_folder)
                ? client.Inbox
                : client.GetFolder(_folder);
            using var openedFolder = new OpenedMailFolder(folder, FolderAccess.ReadWrite);
            var flag = _setToReadIfBatchDeletion ? MessageFlags.Seen : MessageFlags.Deleted;
            openedFolder.AddFlags(new UniqueId(_messageId), flag, true);
            if (flag == MessageFlags.Deleted)
                openedFolder.Expunge();
        }
    }
    public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
    private Stream GetContentSingleTime()
    {
        using var client = _connectionInfo.CreateIMapClient();
        var folder = string.IsNullOrWhiteSpace(_folder)
            ? client.Inbox
            : client.GetFolder(_folder);
        using var openedFolder = new OpenedMailFolder(folder);
        var message = openedFolder.GetMessage(new UniqueId(_messageId));
        var attachment = message.Attachments.Single(i => i.ContentDisposition.FileName == this.Name);

        var ms = new MemoryStream();
        attachment.WriteTo(ms, true);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public override StreamWithResource OpenContent() => new StreamWithResource(GetContent());
}
