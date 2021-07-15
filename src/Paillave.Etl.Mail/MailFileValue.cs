using MailKit;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Paillave.Etl.Mail
{
    public class MailFileValue : FileValueBase<MailFileValueMetadata>
    {
        public override string Name { get; }
        private readonly uint _messageId;
        private readonly string _folder;
        private bool _setToReadIfBatchDeletion;
        private readonly IMailConnectionInfo _connectionInfo;
        private readonly Dictionary<string, bool> _deleted;
        // public IMapFileValue(IMailConnectionInfo connectionInfo, string folder, string fileName)
        //     : this(connectionInfo, folder, fileName, null, null, null, false) { }
        public MailFileValue(IMailConnectionInfo connectionInfo, string folder, string fileName, string connectorCode, string connectorName, string connectionName, bool setToReadIfBatchDeletion, string mailSubject, DateTime receivedDate, string sender, uint messageId, Dictionary<string, bool> deleted)
            : base(new MailFileValueMetadata
            {
                Server = connectionInfo.Server,
                Folder = folder,
                Name = fileName,
                ConnectorCode = connectorCode,
                ConnectionName = connectionName,
                ConnectorName = connectorName,
                MailSubject = mailSubject,
                ReceivedDate = receivedDate
            }) => (Name, _messageId, _connectionInfo, _deleted, _folder, _setToReadIfBatchDeletion) = (fileName, messageId, connectionInfo, deleted, folder, setToReadIfBatchDeletion);
        protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
        protected void DeleteFileSingleTime()
        {
            _deleted[this.Name] = true;
            if (_deleted.Values.All(v => v))
            {
                using (var client = _connectionInfo.CreateIMapClient())
                {
                    var folder = string.IsNullOrWhiteSpace(_folder)
                        ? client.Inbox
                        : client.GetFolder(_folder);
                    var flag = _setToReadIfBatchDeletion ? MessageFlags.Seen : MessageFlags.Deleted;
                    folder.AddFlags(new UniqueId(_messageId), flag, true);
                    if (flag == MessageFlags.Deleted)
                        folder.Expunge();
                }
            }
        }
        public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
        private Stream GetContentSingleTime()
        {
            using (var client = _connectionInfo.CreateIMapClient())
            {
                var folder = string.IsNullOrWhiteSpace(_folder)
                    ? client.Inbox
                    : client.GetFolder(_folder);
                var message = folder.GetMessage(new UniqueId(_messageId));
                var attachment = message.Attachments.Single(i => i.ContentDisposition.FileName == this.Name);

                MemoryStream ms = new MemoryStream();
                attachment.WriteTo(ms, true);
                ms.Seek(0, SeekOrigin.Begin);
                return ms;
            }
        }
    }
    public class MailFileValueMetadata : FileValueMetadataBase
    {
        public string Server { get; set; }
        public string Folder { get; set; }
        public string Name { get; set; }
        public string ConnectorCode { get; set; }
        public string ConnectionName { get; set; }
        public string ConnectorName { get; set; }
        public string MailSubject { get; set; }
        public DateTime ReceivedDate { get; set; }
        public string Sender { get; set; }
    }
}
