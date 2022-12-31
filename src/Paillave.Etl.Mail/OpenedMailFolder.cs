using System;
using System.Threading;
using MailKit.Search;
using System.Collections.Generic;
using MailKit;
using System.Threading.Tasks;
using MimeKit;
using System.IO;
using System.Collections;

namespace Paillave.Etl.Mail;
internal class OpenedMailFolder : IDisposable, IMailFolder
{
    private readonly IMailFolder _mailFolder;

    public OpenedMailFolder(IMailFolder mailFolder, FolderAccess folderAccess = FolderAccess.ReadOnly)
    {
        _mailFolder = mailFolder;
        _mailFolder.Open(folderAccess);
    }

    public event EventHandler<EventArgs> Opened
    {
        add
        {
            _mailFolder.Opened += value;
        }

        remove
        {
            _mailFolder.Opened -= value;
        }
    }

    public event EventHandler<EventArgs> Closed
    {
        add
        {
            _mailFolder.Closed += value;
        }

        remove
        {
            _mailFolder.Closed -= value;
        }
    }

    public event EventHandler<EventArgs> Deleted
    {
        add
        {
            _mailFolder.Deleted += value;
        }

        remove
        {
            _mailFolder.Deleted -= value;
        }
    }

    public event EventHandler<FolderRenamedEventArgs> Renamed
    {
        add
        {
            _mailFolder.Renamed += value;
        }

        remove
        {
            _mailFolder.Renamed -= value;
        }
    }

    public event EventHandler<EventArgs> Subscribed
    {
        add
        {
            _mailFolder.Subscribed += value;
        }

        remove
        {
            _mailFolder.Subscribed -= value;
        }
    }

    public event EventHandler<EventArgs> Unsubscribed
    {
        add
        {
            _mailFolder.Unsubscribed += value;
        }

        remove
        {
            _mailFolder.Unsubscribed -= value;
        }
    }

    public event EventHandler<MessageEventArgs> MessageExpunged
    {
        add
        {
            _mailFolder.MessageExpunged += value;
        }

        remove
        {
            _mailFolder.MessageExpunged -= value;
        }
    }

    public event EventHandler<MessagesVanishedEventArgs> MessagesVanished
    {
        add
        {
            _mailFolder.MessagesVanished += value;
        }

        remove
        {
            _mailFolder.MessagesVanished -= value;
        }
    }

    public event EventHandler<MessageFlagsChangedEventArgs> MessageFlagsChanged
    {
        add
        {
            _mailFolder.MessageFlagsChanged += value;
        }

        remove
        {
            _mailFolder.MessageFlagsChanged -= value;
        }
    }

    public event EventHandler<MessageLabelsChangedEventArgs> MessageLabelsChanged
    {
        add
        {
            _mailFolder.MessageLabelsChanged += value;
        }

        remove
        {
            _mailFolder.MessageLabelsChanged -= value;
        }
    }

    public event EventHandler<AnnotationsChangedEventArgs> AnnotationsChanged
    {
        add
        {
            _mailFolder.AnnotationsChanged += value;
        }

        remove
        {
            _mailFolder.AnnotationsChanged -= value;
        }
    }

    public event EventHandler<MessageSummaryFetchedEventArgs> MessageSummaryFetched
    {
        add
        {
            _mailFolder.MessageSummaryFetched += value;
        }

        remove
        {
            _mailFolder.MessageSummaryFetched -= value;
        }
    }

    public event EventHandler<MetadataChangedEventArgs> MetadataChanged
    {
        add
        {
            _mailFolder.MetadataChanged += value;
        }

        remove
        {
            _mailFolder.MetadataChanged -= value;
        }
    }

    public event EventHandler<ModSeqChangedEventArgs> ModSeqChanged
    {
        add
        {
            _mailFolder.ModSeqChanged += value;
        }

        remove
        {
            _mailFolder.ModSeqChanged -= value;
        }
    }

    public event EventHandler<EventArgs> HighestModSeqChanged
    {
        add
        {
            _mailFolder.HighestModSeqChanged += value;
        }

        remove
        {
            _mailFolder.HighestModSeqChanged -= value;
        }
    }

    public event EventHandler<EventArgs> UidNextChanged
    {
        add
        {
            _mailFolder.UidNextChanged += value;
        }

        remove
        {
            _mailFolder.UidNextChanged -= value;
        }
    }

    public event EventHandler<EventArgs> UidValidityChanged
    {
        add
        {
            _mailFolder.UidValidityChanged += value;
        }

        remove
        {
            _mailFolder.UidValidityChanged -= value;
        }
    }

    public event EventHandler<EventArgs> IdChanged
    {
        add
        {
            _mailFolder.IdChanged += value;
        }

        remove
        {
            _mailFolder.IdChanged -= value;
        }
    }

    public event EventHandler<EventArgs> SizeChanged
    {
        add
        {
            _mailFolder.SizeChanged += value;
        }

        remove
        {
            _mailFolder.SizeChanged -= value;
        }
    }

    public event EventHandler<EventArgs> CountChanged
    {
        add
        {
            _mailFolder.CountChanged += value;
        }

        remove
        {
            _mailFolder.CountChanged -= value;
        }
    }

    public event EventHandler<EventArgs> RecentChanged
    {
        add
        {
            _mailFolder.RecentChanged += value;
        }

        remove
        {
            _mailFolder.RecentChanged -= value;
        }
    }

    public event EventHandler<EventArgs> UnreadChanged
    {
        add
        {
            _mailFolder.UnreadChanged += value;
        }

        remove
        {
            _mailFolder.UnreadChanged -= value;
        }
    }

    private bool disposedValue;

    public object SyncRoot => _mailFolder.SyncRoot;

    public IMailFolder ParentFolder => _mailFolder.ParentFolder;

    public FolderAttributes Attributes => _mailFolder.Attributes;

    public AnnotationAccess AnnotationAccess => _mailFolder.AnnotationAccess;

    public AnnotationScope AnnotationScopes => _mailFolder.AnnotationScopes;

    public uint MaxAnnotationSize => _mailFolder.MaxAnnotationSize;

    public MessageFlags PermanentFlags => _mailFolder.PermanentFlags;

    public IReadOnlySet<string> PermanentKeywords => _mailFolder.PermanentKeywords;

    public MessageFlags AcceptedFlags => _mailFolder.AcceptedFlags;

    public IReadOnlySet<string> AcceptedKeywords => _mailFolder.AcceptedKeywords;

    public char DirectorySeparator => _mailFolder.DirectorySeparator;

    public FolderAccess Access => _mailFolder.Access;

    public bool IsNamespace => _mailFolder.IsNamespace;

    public string FullName => _mailFolder.FullName;

    public string Name => _mailFolder.Name;

    public string Id => _mailFolder.Id;

    public bool IsSubscribed => _mailFolder.IsSubscribed;

    public bool IsOpen => _mailFolder.IsOpen;

    public bool Exists => _mailFolder.Exists;

    public ulong HighestModSeq => _mailFolder.HighestModSeq;

    public uint UidValidity => _mailFolder.UidValidity;

    public UniqueId? UidNext => _mailFolder.UidNext;

    public uint? AppendLimit => _mailFolder.AppendLimit;

    public ulong? Size => _mailFolder.Size;

    public int FirstUnread => _mailFolder.FirstUnread;

    public int Unread => _mailFolder.Unread;

    public int Recent => _mailFolder.Recent;

    public int Count => _mailFolder.Count;

    public HashSet<ThreadingAlgorithm> ThreadingAlgorithms => _mailFolder.ThreadingAlgorithms;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _mailFolder.Close();
                // TODO: dispose managed state (managed objects)
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public bool Supports(FolderFeature feature)
    {
        return _mailFolder.Supports(feature);
    }

    public FolderAccess Open(FolderAccess access, uint uidValidity, ulong highestModSeq, IList<UniqueId> uids, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Open(access, uidValidity, highestModSeq, uids, cancellationToken);
    }

    public Task<FolderAccess> OpenAsync(FolderAccess access, uint uidValidity, ulong highestModSeq, IList<UniqueId> uids, CancellationToken cancellationToken = default)
    {
        return _mailFolder.OpenAsync(access, uidValidity, highestModSeq, uids, cancellationToken);
    }

    public FolderAccess Open(FolderAccess access, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Open(access, cancellationToken);
    }

    public Task<FolderAccess> OpenAsync(FolderAccess access, CancellationToken cancellationToken = default)
    {
        return _mailFolder.OpenAsync(access, cancellationToken);
    }

    public void Close(bool expunge = false, CancellationToken cancellationToken = default)
    {
        _mailFolder.Close(expunge, cancellationToken);
    }

    public Task CloseAsync(bool expunge = false, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CloseAsync(expunge, cancellationToken);
    }

    public IMailFolder Create(string name, bool isMessageFolder, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Create(name, isMessageFolder, cancellationToken);
    }

    public Task<IMailFolder> CreateAsync(string name, bool isMessageFolder, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CreateAsync(name, isMessageFolder, cancellationToken);
    }

    public IMailFolder Create(string name, IEnumerable<SpecialFolder> specialUses, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Create(name, specialUses, cancellationToken);
    }

    public Task<IMailFolder> CreateAsync(string name, IEnumerable<SpecialFolder> specialUses, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CreateAsync(name, specialUses, cancellationToken);
    }

    public IMailFolder Create(string name, SpecialFolder specialUse, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Create(name, specialUse, cancellationToken);
    }

    public Task<IMailFolder> CreateAsync(string name, SpecialFolder specialUse, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CreateAsync(name, specialUse, cancellationToken);
    }

    public void Rename(IMailFolder parent, string name, CancellationToken cancellationToken = default)
    {
        _mailFolder.Rename(parent, name, cancellationToken);
    }

    public Task RenameAsync(IMailFolder parent, string name, CancellationToken cancellationToken = default)
    {
        return _mailFolder.RenameAsync(parent, name, cancellationToken);
    }

    public void Delete(CancellationToken cancellationToken = default)
    {
        _mailFolder.Delete(cancellationToken);
    }

    public Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        return _mailFolder.DeleteAsync(cancellationToken);
    }

    public void Subscribe(CancellationToken cancellationToken = default)
    {
        _mailFolder.Subscribe(cancellationToken);
    }

    public Task SubscribeAsync(CancellationToken cancellationToken = default)
    {
        return _mailFolder.SubscribeAsync(cancellationToken);
    }

    public void Unsubscribe(CancellationToken cancellationToken = default)
    {
        _mailFolder.Unsubscribe(cancellationToken);
    }

    public Task UnsubscribeAsync(CancellationToken cancellationToken = default)
    {
        return _mailFolder.UnsubscribeAsync(cancellationToken);
    }

    public IList<IMailFolder> GetSubfolders(StatusItems items, bool subscribedOnly = false, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetSubfolders(items, subscribedOnly, cancellationToken);
    }

    public Task<IList<IMailFolder>> GetSubfoldersAsync(StatusItems items, bool subscribedOnly = false, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetSubfoldersAsync(items, subscribedOnly, cancellationToken);
    }

    public IList<IMailFolder> GetSubfolders(bool subscribedOnly = false, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetSubfolders(subscribedOnly, cancellationToken);
    }

    public Task<IList<IMailFolder>> GetSubfoldersAsync(bool subscribedOnly = false, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetSubfoldersAsync(subscribedOnly, cancellationToken);
    }

    public IMailFolder GetSubfolder(string name, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetSubfolder(name, cancellationToken);
    }

    public Task<IMailFolder> GetSubfolderAsync(string name, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetSubfolderAsync(name, cancellationToken);
    }

    public void Check(CancellationToken cancellationToken = default)
    {
        _mailFolder.Check(cancellationToken);
    }

    public Task CheckAsync(CancellationToken cancellationToken = default)
    {
        return _mailFolder.CheckAsync(cancellationToken);
    }

    public void Status(StatusItems items, CancellationToken cancellationToken = default)
    {
        _mailFolder.Status(items, cancellationToken);
    }

    public Task StatusAsync(StatusItems items, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StatusAsync(items, cancellationToken);
    }

    public AccessControlList GetAccessControlList(CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetAccessControlList(cancellationToken);
    }

    public Task<AccessControlList> GetAccessControlListAsync(CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetAccessControlListAsync(cancellationToken);
    }

    public AccessRights GetAccessRights(string name, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetAccessRights(name, cancellationToken);
    }

    public Task<AccessRights> GetAccessRightsAsync(string name, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetAccessRightsAsync(name, cancellationToken);
    }

    public AccessRights GetMyAccessRights(CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetMyAccessRights(cancellationToken);
    }

    public Task<AccessRights> GetMyAccessRightsAsync(CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetMyAccessRightsAsync(cancellationToken);
    }

    public void AddAccessRights(string name, AccessRights rights, CancellationToken cancellationToken = default)
    {
        _mailFolder.AddAccessRights(name, rights, cancellationToken);
    }

    public Task AddAccessRightsAsync(string name, AccessRights rights, CancellationToken cancellationToken = default)
    {
        return _mailFolder.AddAccessRightsAsync(name, rights, cancellationToken);
    }

    public void RemoveAccessRights(string name, AccessRights rights, CancellationToken cancellationToken = default)
    {
        _mailFolder.RemoveAccessRights(name, rights, cancellationToken);
    }

    public Task RemoveAccessRightsAsync(string name, AccessRights rights, CancellationToken cancellationToken = default)
    {
        return _mailFolder.RemoveAccessRightsAsync(name, rights, cancellationToken);
    }

    public void SetAccessRights(string name, AccessRights rights, CancellationToken cancellationToken = default)
    {
        _mailFolder.SetAccessRights(name, rights, cancellationToken);
    }

    public Task SetAccessRightsAsync(string name, AccessRights rights, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SetAccessRightsAsync(name, rights, cancellationToken);
    }

    public void RemoveAccess(string name, CancellationToken cancellationToken = default)
    {
        _mailFolder.RemoveAccess(name, cancellationToken);
    }

    public Task RemoveAccessAsync(string name, CancellationToken cancellationToken = default)
    {
        return _mailFolder.RemoveAccessAsync(name, cancellationToken);
    }

    public FolderQuota GetQuota(CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetQuota(cancellationToken);
    }

    public Task<FolderQuota> GetQuotaAsync(CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetQuotaAsync(cancellationToken);
    }

    public FolderQuota SetQuota(uint? messageLimit, uint? storageLimit, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SetQuota(messageLimit, storageLimit, cancellationToken);
    }

    public Task<FolderQuota> SetQuotaAsync(uint? messageLimit, uint? storageLimit, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SetQuotaAsync(messageLimit, storageLimit, cancellationToken);
    }

    public string GetMetadata(MetadataTag tag, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetMetadata(tag, cancellationToken);
    }

    public Task<string> GetMetadataAsync(MetadataTag tag, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetMetadataAsync(tag, cancellationToken);
    }

    public MetadataCollection GetMetadata(IEnumerable<MetadataTag> tags, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetMetadata(tags, cancellationToken);
    }

    public Task<MetadataCollection> GetMetadataAsync(IEnumerable<MetadataTag> tags, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetMetadataAsync(tags, cancellationToken);
    }

    public MetadataCollection GetMetadata(MetadataOptions options, IEnumerable<MetadataTag> tags, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetMetadata(options, tags, cancellationToken);
    }

    public Task<MetadataCollection> GetMetadataAsync(MetadataOptions options, IEnumerable<MetadataTag> tags, CancellationToken cancellationToken = default)
    {
        return _mailFolder.GetMetadataAsync(options, tags, cancellationToken);
    }

    public void SetMetadata(MetadataCollection metadata, CancellationToken cancellationToken = default)
    {
        _mailFolder.SetMetadata(metadata, cancellationToken);
    }

    public Task SetMetadataAsync(MetadataCollection metadata, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SetMetadataAsync(metadata, cancellationToken);
    }

    public void Expunge(CancellationToken cancellationToken = default)
    {
        _mailFolder.Expunge(cancellationToken);
    }

    public Task ExpungeAsync(CancellationToken cancellationToken = default)
    {
        return _mailFolder.ExpungeAsync(cancellationToken);
    }

    public void Expunge(IList<UniqueId> uids, CancellationToken cancellationToken = default)
    {
        _mailFolder.Expunge(uids, cancellationToken);
    }

    public Task ExpungeAsync(IList<UniqueId> uids, CancellationToken cancellationToken = default)
    {
        return _mailFolder.ExpungeAsync(uids, cancellationToken);
    }

    public UniqueId? Append(IAppendRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Append(request, cancellationToken);
    }

    public Task<UniqueId?> AppendAsync(IAppendRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.AppendAsync(request, cancellationToken);
    }

    public UniqueId? Append(FormatOptions options, IAppendRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Append(options, request, cancellationToken);
    }

    public Task<UniqueId?> AppendAsync(FormatOptions options, IAppendRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.AppendAsync(options, request, cancellationToken);
    }

    public IList<UniqueId> Append(IList<IAppendRequest> requests, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Append(requests, cancellationToken);
    }

    public Task<IList<UniqueId>> AppendAsync(IList<IAppendRequest> requests, CancellationToken cancellationToken = default)
    {
        return _mailFolder.AppendAsync(requests, cancellationToken);
    }

    public IList<UniqueId> Append(FormatOptions options, IList<IAppendRequest> requests, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Append(options, requests, cancellationToken);
    }

    public Task<IList<UniqueId>> AppendAsync(FormatOptions options, IList<IAppendRequest> requests, CancellationToken cancellationToken = default)
    {
        return _mailFolder.AppendAsync(options, requests, cancellationToken);
    }

    public UniqueId? Replace(UniqueId uid, IReplaceRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Replace(uid, request, cancellationToken);
    }

    public Task<UniqueId?> ReplaceAsync(UniqueId uid, IReplaceRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.ReplaceAsync(uid, request, cancellationToken);
    }

    public UniqueId? Replace(FormatOptions options, UniqueId uid, IReplaceRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Replace(options, uid, request, cancellationToken);
    }

    public Task<UniqueId?> ReplaceAsync(FormatOptions options, UniqueId uid, IReplaceRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.ReplaceAsync(options, uid, request, cancellationToken);
    }

    public UniqueId? Replace(int index, IReplaceRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Replace(index, request, cancellationToken);
    }

    public Task<UniqueId?> ReplaceAsync(int index, IReplaceRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.ReplaceAsync(index, request, cancellationToken);
    }

    public UniqueId? Replace(FormatOptions options, int index, IReplaceRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Replace(options, index, request, cancellationToken);
    }

    public Task<UniqueId?> ReplaceAsync(FormatOptions options, int index, IReplaceRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.ReplaceAsync(options, index, request, cancellationToken);
    }

    public UniqueId? CopyTo(UniqueId uid, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CopyTo(uid, destination, cancellationToken);
    }

    public Task<UniqueId?> CopyToAsync(UniqueId uid, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CopyToAsync(uid, destination, cancellationToken);
    }

    public UniqueIdMap CopyTo(IList<UniqueId> uids, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CopyTo(uids, destination, cancellationToken);
    }

    public Task<UniqueIdMap> CopyToAsync(IList<UniqueId> uids, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CopyToAsync(uids, destination, cancellationToken);
    }

    public UniqueId? MoveTo(UniqueId uid, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.MoveTo(uid, destination, cancellationToken);
    }

    public Task<UniqueId?> MoveToAsync(UniqueId uid, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.MoveToAsync(uid, destination, cancellationToken);
    }

    public UniqueIdMap MoveTo(IList<UniqueId> uids, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.MoveTo(uids, destination, cancellationToken);
    }

    public Task<UniqueIdMap> MoveToAsync(IList<UniqueId> uids, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.MoveToAsync(uids, destination, cancellationToken);
    }

    public void CopyTo(int index, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        _mailFolder.CopyTo(index, destination, cancellationToken);
    }

    public Task CopyToAsync(int index, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CopyToAsync(index, destination, cancellationToken);
    }

    public void CopyTo(IList<int> indexes, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        _mailFolder.CopyTo(indexes, destination, cancellationToken);
    }

    public Task CopyToAsync(IList<int> indexes, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.CopyToAsync(indexes, destination, cancellationToken);
    }

    public void MoveTo(int index, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        _mailFolder.MoveTo(index, destination, cancellationToken);
    }

    public Task MoveToAsync(int index, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.MoveToAsync(index, destination, cancellationToken);
    }

    public void MoveTo(IList<int> indexes, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        _mailFolder.MoveTo(indexes, destination, cancellationToken);
    }

    public Task MoveToAsync(IList<int> indexes, IMailFolder destination, CancellationToken cancellationToken = default)
    {
        return _mailFolder.MoveToAsync(indexes, destination, cancellationToken);
    }

    public IList<IMessageSummary> Fetch(IList<UniqueId> uids, IFetchRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Fetch(uids, request, cancellationToken);
    }

    public Task<IList<IMessageSummary>> FetchAsync(IList<UniqueId> uids, IFetchRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.FetchAsync(uids, request, cancellationToken);
    }

    public IList<IMessageSummary> Fetch(IList<int> indexes, IFetchRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Fetch(indexes, request, cancellationToken);
    }

    public Task<IList<IMessageSummary>> FetchAsync(IList<int> indexes, IFetchRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.FetchAsync(indexes, request, cancellationToken);
    }

    public IList<IMessageSummary> Fetch(int min, int max, IFetchRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Fetch(min, max, request, cancellationToken);
    }

    public Task<IList<IMessageSummary>> FetchAsync(int min, int max, IFetchRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.FetchAsync(min, max, request, cancellationToken);
    }

    public HeaderList GetHeaders(UniqueId uid, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetHeaders(uid, cancellationToken, progress);
    }

    public Task<HeaderList> GetHeadersAsync(UniqueId uid, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetHeadersAsync(uid, cancellationToken, progress);
    }

    public HeaderList GetHeaders(UniqueId uid, BodyPart part, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetHeaders(uid, part, cancellationToken, progress);
    }

    public Task<HeaderList> GetHeadersAsync(UniqueId uid, BodyPart part, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetHeadersAsync(uid, part, cancellationToken, progress);
    }

    public HeaderList GetHeaders(int index, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetHeaders(index, cancellationToken, progress);
    }

    public Task<HeaderList> GetHeadersAsync(int index, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetHeadersAsync(index, cancellationToken, progress);
    }

    public HeaderList GetHeaders(int index, BodyPart part, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetHeaders(index, part, cancellationToken, progress);
    }

    public Task<HeaderList> GetHeadersAsync(int index, BodyPart part, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetHeadersAsync(index, part, cancellationToken, progress);
    }

    public MimeMessage GetMessage(UniqueId uid, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetMessage(uid, cancellationToken, progress);
    }

    public Task<MimeMessage> GetMessageAsync(UniqueId uid, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetMessageAsync(uid, cancellationToken, progress);
    }

    public MimeMessage GetMessage(int index, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetMessage(index, cancellationToken, progress);
    }

    public Task<MimeMessage> GetMessageAsync(int index, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetMessageAsync(index, cancellationToken, progress);
    }

    public MimeEntity GetBodyPart(UniqueId uid, BodyPart part, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetBodyPart(uid, part, cancellationToken, progress);
    }

    public Task<MimeEntity> GetBodyPartAsync(UniqueId uid, BodyPart part, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetBodyPartAsync(uid, part, cancellationToken, progress);
    }

    public MimeEntity GetBodyPart(int index, BodyPart part, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetBodyPart(index, part, cancellationToken, progress);
    }

    public Task<MimeEntity> GetBodyPartAsync(int index, BodyPart part, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetBodyPartAsync(index, part, cancellationToken, progress);
    }

    public Stream GetStream(UniqueId uid, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStream(uid, offset, count, cancellationToken, progress);
    }

    public Task<Stream> GetStreamAsync(UniqueId uid, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStreamAsync(uid, offset, count, cancellationToken, progress);
    }

    public Stream GetStream(int index, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStream(index, offset, count, cancellationToken, progress);
    }

    public Task<Stream> GetStreamAsync(int index, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStreamAsync(index, offset, count, cancellationToken, progress);
    }

    public Stream GetStream(UniqueId uid, BodyPart part, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStream(uid, part, offset, count, cancellationToken, progress);
    }

    public Task<Stream> GetStreamAsync(UniqueId uid, BodyPart part, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStreamAsync(uid, part, offset, count, cancellationToken, progress);
    }

    public Stream GetStream(int index, BodyPart part, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStream(index, part, offset, count, cancellationToken, progress);
    }

    public Task<Stream> GetStreamAsync(int index, BodyPart part, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStreamAsync(index, part, offset, count, cancellationToken, progress);
    }

    public Stream GetStream(UniqueId uid, string section, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStream(uid, section, cancellationToken, progress);
    }

    public Task<Stream> GetStreamAsync(UniqueId uid, string section, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStreamAsync(uid, section, cancellationToken, progress);
    }

    public Stream GetStream(UniqueId uid, string section, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStream(uid, section, offset, count, cancellationToken, progress);
    }

    public Task<Stream> GetStreamAsync(UniqueId uid, string section, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStreamAsync(uid, section, offset, count, cancellationToken, progress);
    }

    public Stream GetStream(int index, string section, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStream(index, section, cancellationToken, progress);
    }

    public Task<Stream> GetStreamAsync(int index, string section, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStreamAsync(index, section, cancellationToken, progress);
    }

    public Stream GetStream(int index, string section, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStream(index, section, offset, count, cancellationToken, progress);
    }

    public Task<Stream> GetStreamAsync(int index, string section, int offset, int count, CancellationToken cancellationToken = default, ITransferProgress progress = null)
    {
        return _mailFolder.GetStreamAsync(index, section, offset, count, cancellationToken, progress);
    }

    public bool Store(UniqueId uid, IStoreFlagsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(uid, request, cancellationToken);
    }

    public Task<bool> StoreAsync(UniqueId uid, IStoreFlagsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(uid, request, cancellationToken);
    }

    public IList<UniqueId> Store(IList<UniqueId> uids, IStoreFlagsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(uids, request, cancellationToken);
    }

    public Task<IList<UniqueId>> StoreAsync(IList<UniqueId> uids, IStoreFlagsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(uids, request, cancellationToken);
    }

    public bool Store(int index, IStoreFlagsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(index, request, cancellationToken);
    }

    public Task<bool> StoreAsync(int index, IStoreFlagsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(index, request, cancellationToken);
    }

    public IList<int> Store(IList<int> indexes, IStoreFlagsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(indexes, request, cancellationToken);
    }

    public Task<IList<int>> StoreAsync(IList<int> indexes, IStoreFlagsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(indexes, request, cancellationToken);
    }

    public bool Store(UniqueId uid, IStoreLabelsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(uid, request, cancellationToken);
    }

    public Task<bool> StoreAsync(UniqueId uid, IStoreLabelsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(uid, request, cancellationToken);
    }

    public IList<UniqueId> Store(IList<UniqueId> uids, IStoreLabelsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(uids, request, cancellationToken);
    }

    public Task<IList<UniqueId>> StoreAsync(IList<UniqueId> uids, IStoreLabelsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(uids, request, cancellationToken);
    }

    public bool Store(int index, IStoreLabelsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(index, request, cancellationToken);
    }

    public Task<bool> StoreAsync(int index, IStoreLabelsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(index, request, cancellationToken);
    }

    public IList<int> Store(IList<int> indexes, IStoreLabelsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(indexes, request, cancellationToken);
    }

    public Task<IList<int>> StoreAsync(IList<int> indexes, IStoreLabelsRequest request, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(indexes, request, cancellationToken);
    }

    public void Store(UniqueId uid, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        _mailFolder.Store(uid, annotations, cancellationToken);
    }

    public Task StoreAsync(UniqueId uid, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(uid, annotations, cancellationToken);
    }

    public void Store(IList<UniqueId> uids, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        _mailFolder.Store(uids, annotations, cancellationToken);
    }

    public Task StoreAsync(IList<UniqueId> uids, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(uids, annotations, cancellationToken);
    }

    public IList<UniqueId> Store(IList<UniqueId> uids, ulong modseq, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(uids, modseq, annotations, cancellationToken);
    }

    public Task<IList<UniqueId>> StoreAsync(IList<UniqueId> uids, ulong modseq, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(uids, modseq, annotations, cancellationToken);
    }

    public void Store(int index, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        _mailFolder.Store(index, annotations, cancellationToken);
    }

    public Task StoreAsync(int index, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(index, annotations, cancellationToken);
    }

    public void Store(IList<int> indexes, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        _mailFolder.Store(indexes, annotations, cancellationToken);
    }

    public Task StoreAsync(IList<int> indexes, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(indexes, annotations, cancellationToken);
    }

    public IList<int> Store(IList<int> indexes, ulong modseq, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Store(indexes, modseq, annotations, cancellationToken);
    }

    public Task<IList<int>> StoreAsync(IList<int> indexes, ulong modseq, IList<Annotation> annotations, CancellationToken cancellationToken = default)
    {
        return _mailFolder.StoreAsync(indexes, modseq, annotations, cancellationToken);
    }

    public IList<UniqueId> Search(SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Search(query, cancellationToken);
    }

    public Task<IList<UniqueId>> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SearchAsync(query, cancellationToken);
    }

    public IList<UniqueId> Search(IList<UniqueId> uids, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Search(uids, query, cancellationToken);
    }

    public Task<IList<UniqueId>> SearchAsync(IList<UniqueId> uids, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SearchAsync(uids, query, cancellationToken);
    }

    public SearchResults Search(SearchOptions options, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Search(options, query, cancellationToken);
    }

    public Task<SearchResults> SearchAsync(SearchOptions options, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SearchAsync(options, query, cancellationToken);
    }

    public SearchResults Search(SearchOptions options, IList<UniqueId> uids, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Search(options, uids, query, cancellationToken);
    }

    public Task<SearchResults> SearchAsync(SearchOptions options, IList<UniqueId> uids, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SearchAsync(options, uids, query, cancellationToken);
    }

    public IList<UniqueId> Sort(SearchQuery query, IList<OrderBy> orderBy, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Sort(query, orderBy, cancellationToken);
    }

    public Task<IList<UniqueId>> SortAsync(SearchQuery query, IList<OrderBy> orderBy, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SortAsync(query, orderBy, cancellationToken);
    }

    public IList<UniqueId> Sort(IList<UniqueId> uids, SearchQuery query, IList<OrderBy> orderBy, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Sort(uids, query, orderBy, cancellationToken);
    }

    public Task<IList<UniqueId>> SortAsync(IList<UniqueId> uids, SearchQuery query, IList<OrderBy> orderBy, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SortAsync(uids, query, orderBy, cancellationToken);
    }

    public SearchResults Sort(SearchOptions options, SearchQuery query, IList<OrderBy> orderBy, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Sort(options, query, orderBy, cancellationToken);
    }

    public Task<SearchResults> SortAsync(SearchOptions options, SearchQuery query, IList<OrderBy> orderBy, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SortAsync(options, query, orderBy, cancellationToken);
    }

    public SearchResults Sort(SearchOptions options, IList<UniqueId> uids, SearchQuery query, IList<OrderBy> orderBy, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Sort(options, uids, query, orderBy, cancellationToken);
    }

    public Task<SearchResults> SortAsync(SearchOptions options, IList<UniqueId> uids, SearchQuery query, IList<OrderBy> orderBy, CancellationToken cancellationToken = default)
    {
        return _mailFolder.SortAsync(options, uids, query, orderBy, cancellationToken);
    }

    public IList<MessageThread> Thread(ThreadingAlgorithm algorithm, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Thread(algorithm, query, cancellationToken);
    }

    public Task<IList<MessageThread>> ThreadAsync(ThreadingAlgorithm algorithm, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.ThreadAsync(algorithm, query, cancellationToken);
    }

    public IList<MessageThread> Thread(IList<UniqueId> uids, ThreadingAlgorithm algorithm, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.Thread(uids, algorithm, query, cancellationToken);
    }

    public Task<IList<MessageThread>> ThreadAsync(IList<UniqueId> uids, ThreadingAlgorithm algorithm, SearchQuery query, CancellationToken cancellationToken = default)
    {
        return _mailFolder.ThreadAsync(uids, algorithm, query, cancellationToken);
    }

    public IEnumerator<MimeMessage> GetEnumerator()
    {
        return _mailFolder.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_mailFolder).GetEnumerator();
    }
}
