using MailKit;
using Paillave.Etl.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Paillave.Etl.Mail;

public class MailFileValue(IMailConnectionInfo connectionInfo, string folder, string fileName, bool setToReadIfBatchDeletion, uint messageId, Dictionary<string, bool> deleted, string subject) : FileValueBase
{
    public override string Name => fileName;
    public string Subject => subject;
    protected override void DeleteFile() => ActionRunner.TryExecute(connectionInfo.MaxAttempts, DeleteFileSingleTime);
    protected void DeleteFileSingleTime()
    {
        deleted[this.Name] = true;
        if (deleted.Values.All(v => v))
        {
            using var client = connectionInfo.CreateIMapClient();
            var mailFolder = string.IsNullOrWhiteSpace(folder)
                ? client.Inbox
                : client.GetFolder(folder);
            using var openedFolder = new OpenedMailFolder(mailFolder, FolderAccess.ReadWrite);
            var flag = setToReadIfBatchDeletion ? MessageFlags.Seen : MessageFlags.Deleted;
            openedFolder.AddFlags(new UniqueId(messageId), flag, true);
            if (flag == MessageFlags.Deleted)
                openedFolder.Expunge();
        }
    }
    public override Stream GetContent() => ActionRunner.TryExecute(connectionInfo.MaxAttempts, GetContentSingleTime);
    private Stream GetContentSingleTime()
    {
        using var client = connectionInfo.CreateIMapClient();
        var mailFolder = string.IsNullOrWhiteSpace(folder)
            ? client.Inbox
            : client.GetFolder(folder);
        using var openedFolder = new OpenedMailFolder(mailFolder);
        var message = openedFolder.GetMessage(new UniqueId(messageId));
        var attachment = message.Attachments.Single(i => i.ContentDisposition.FileName == this.Name);

        var ms = new MemoryStream();
        attachment.WriteTo(ms, true);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public override StreamWithResource OpenContent() => new(GetContent());
}
