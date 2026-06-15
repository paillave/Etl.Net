using Paillave.Etl.Core;
using System;
using System.IO;

namespace Paillave.Etl.GraphApi;

public class GraphApiOneDriveFileValue : FileValueBase
{
    public override string Name { get; }
    public string DriveId { get; }
    public string DriveItemId { get; }
    public string FolderPath { get; }
    private readonly IGraphApiConnectionInfo _connectionInfo;

    internal GraphApiOneDriveFileValue(IGraphApiConnectionInfo connectionInfo, string driveId, string driveItemId, string name, string folderPath)
        => (Name, DriveId, DriveItemId, FolderPath, _connectionInfo) = (name, driveId, driveItemId, folderPath, connectionInfo);

    protected override void DeleteFile() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, DeleteFileSingleTime);
    private void DeleteFileSingleTime()
    {
        using var graphClient = _connectionInfo.CreateGraphApiClient();
        graphClient.Drives[DriveId].Items[DriveItemId].DeleteAsync().GetAwaiter().GetResult();
    }

    public override Stream GetContent() => ActionRunner.TryExecute(_connectionInfo.MaxAttempts, GetContentSingleTime);
    private Stream GetContentSingleTime()
    {
        using var graphClient = _connectionInfo.CreateGraphApiClient();
        var stream = graphClient.Drives[DriveId].Items[DriveItemId].Content
            .GetAsync().GetAwaiter().GetResult() ?? throw new Exception("Drive item content not found");
        var ms = new MemoryStream();
        stream.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public override StreamWithResource OpenContent() => new(GetContent());
}
