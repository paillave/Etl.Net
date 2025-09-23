using System.IO;
using Renci.SshNet;
using Paillave.Etl.Core;

namespace Paillave.Etl.Sftp;

public class SftpFileValue : FileValueBase
{
    public override string Name { get; }
    private readonly string _folder;
    private readonly ISftpConnectionInfo _connectionInfo;

    public SftpFileValue(ISftpConnectionInfo connectionInfo, string folder, string fileName)
        => (Name, _folder, _connectionInfo) = (fileName, folder, connectionInfo);
    protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
    protected void DeleteFileSingleTime()
    {
        var connectionInfo = _connectionInfo.CreateConnectionInfo();
        using (var client = new SftpClient(connectionInfo))
        {
            client.Connect();
            client.DeleteFile(StringEx.ConcatenatePath(_folder, Name));
        }
    }
    public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
    private Stream GetContentSingleTime()
    {
        var connectionInfo = _connectionInfo.CreateConnectionInfo();
        using (var client = new SftpClient(connectionInfo))
        {
            client.Connect();
            return new MemoryStream(client.ReadAllBytes(StringEx.ConcatenatePath(_folder, Name)));
        }
    }
    private StreamWithResource OpenContentSingleTime()
    {
        var connectionInfo = _connectionInfo.CreateConnectionInfo();
        var client = new SftpClient(connectionInfo);
        client.Connect();
        return new StreamWithResource(client.OpenRead(StringEx.ConcatenatePath(_folder, Name)), client);
    }

    public override StreamWithResource OpenContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, OpenContentSingleTime);
}
