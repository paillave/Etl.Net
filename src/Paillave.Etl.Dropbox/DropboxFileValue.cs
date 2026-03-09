using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.Dropbox;

public class DropboxFileValue : FileValueBase
{
    public override string Name { get; }
    private readonly string _folder;
    private readonly IDropboxConnectionInfo _connectionInfo;

    // public DropboxFileValue(IDropboxConnectionInfo connectionInfo, string folder, string fileName)
    //     : this(connectionInfo, folder, fileName, null) { }
    public DropboxFileValue(IDropboxConnectionInfo connectionInfo, string folder, string fileName)
         => (Name, _folder, _connectionInfo) = (fileName, folder, connectionInfo);
    protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
    protected void DeleteFileSingleTime()
    {
        var path = $"/{Path.Combine(_folder ?? "", Name ?? "")}".Replace("\\", "/").Replace("//", "/");
        using (var client = _connectionInfo.CreateConnectionInfo())
            client.Files.DeleteV2Async(path).Wait();
    }
    public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
    public override StreamWithResource OpenContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, OpenContentSingleTime);
    private Stream GetContentSingleTime()
    {
        var client = _connectionInfo.CreateConnectionInfo();
        {
            var path = $"/{Path.Combine(_folder ?? "", Name ?? "")}".Replace("\\", "/").Replace("//", "/");
            var response = client.Files.DownloadAsync(path).Result;
            var dl = response.GetContentAsByteArrayAsync().Result;
            return new MemoryStream(dl);
        }
    }
    private StreamWithResource OpenContentSingleTime()
    {
        var client = _connectionInfo.CreateConnectionInfo();
        var path = $"/{Path.Combine(_folder ?? "", Name ?? "")}".Replace("\\", "/").Replace("//", "/");
        var response = client.Files.DownloadAsync(path).Result;
        return new StreamWithResource(response.GetContentAsStreamAsync().Result, client);
    }
}
