using System.IO;
using Paillave.Etl.Core;

namespace Paillave.Etl.S3;
public class S3FileValue : FileValueBase<S3FileValueMetadata>
{
    public override string Name { get; }
    private readonly string _folder;
    private readonly IS3ConnectionInfo _connectionInfo;

    public S3FileValue(IS3ConnectionInfo connectionInfo, string folder, string fileName)
        : this(connectionInfo, folder, fileName, null, null, null) { }
    public S3FileValue(IS3ConnectionInfo connectionInfo, string folder, string fileName, string connectorCode, string connectionName, string connectorName)
        : base(new S3FileValueMetadata
        {
            ServiceUrl = connectionInfo.ServiceUrl,
            Bucket = connectionInfo.Bucket,
            Folder = folder,
            Name = fileName,
            ConnectorCode = connectorCode,
            ConnectionName = connectionName,
            ConnectorName = connectorName
        }) => (Name, _folder, _connectionInfo) = (fileName, folder, connectionInfo);
    protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
    protected void DeleteFileSingleTime()
    {
        using (var client = _connectionInfo.CreateBucketConnection())
        {
            client.DeleteAsync(Path.Combine(_folder, Name)).Wait();
        }
    }
    public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
    private Stream GetContentSingleTime()
    {
        using (S3Bucket? client = _connectionInfo.CreateBucketConnection())
        {
            var ms = new MemoryStream();
            using (var stream = client.DownloadAsync(Path.Combine(_folder, Name)).Result)
                stream.CopyTo(ms);
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
    private StreamWithResource OpenContentSingleTime()
    {
        var client = _connectionInfo.CreateBucketConnection();
        return new StreamWithResource(client.DownloadAsync(Path.Combine(_folder, Name)).Result, client);
    }

    public override StreamWithResource OpenContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, OpenContentSingleTime);
}
public class S3FileValueMetadata : FileValueMetadataBase
{
    public string Bucket { get; set; }
    public string ServiceUrl { get; set; }
    public string Folder { get; set; }
    public string Name { get; set; }
}
