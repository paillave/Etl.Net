using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.GraphApi;

public class GraphApiOneDriveAdapterProviderParameters
{
    /// <summary>Folder path inside the drive (e.g. "Reports/2024"). Leave empty for root.</summary>
    public string? FolderPath { get; set; }
    public string? FileNamePattern { get; set; }
    public bool Recursive { get; set; }

    // Drive source — set at most one; if none is set, uses the connection UserId personal OneDrive.

    /// <summary>
    /// Full SharePoint site URL, e.g. "https://contoso.sharepoint.com/sites/myteam".
    /// Takes precedence over SiteId.
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>SharePoint site ID in Graph format (host,siteGuid,webGuid).</summary>
    public string? SiteId { get; set; }

    /// <summary>
    /// Display name or email of the Microsoft 365 group / Teams team,
    /// e.g. "My Team" or "myteam@contoso.com". Takes precedence over GroupId.
    /// </summary>
    public string? GroupDisplayNameOrEmail { get; set; }

    /// <summary>GUID of the Microsoft 365 group / Teams team.</summary>
    public string? GroupId { get; set; }

    /// <summary>Direct OneDrive/SharePoint drive ID. Overrides all other drive source options.</summary>
    public string? DriveId { get; set; }

    /// <summary>
    /// SharePoint/OneDrive sharing URL (e.g. "https://contoso.sharepoint.com/:f:/g/Abc123...").
    /// When set, the drive and root folder are resolved from this link; DriveId/SiteUrl/etc. are ignored.
    /// FolderPath is treated as a sub-path inside the shared folder.
    /// </summary>
    public string? SharingUrl { get; set; }
}

public class GraphApiOneDriveFileValueProvider(string code, string name, string connectionName,
    GraphApiAdapterConnectionParameters connectionParameters,
    GraphApiOneDriveAdapterProviderParameters providerParameters)
    : FileValueProviderBase<GraphApiAdapterConnectionParameters, GraphApiOneDriveAdapterProviderParameters>(code, name, connectionName, connectionParameters, providerParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    private class FileSpecificData
    {
        public required string DriveId { get; set; }
        public required string DriveItemId { get; set; }
        public required string FileName { get; set; }
        public required string FolderPath { get; set; }
    }

    protected override void Provide(object input, Action<IFileValue, FileReference> pushFileValue,
        GraphApiAdapterConnectionParameters connectionParameters,
        GraphApiOneDriveAdapterProviderParameters providerParameters,
        CancellationToken cancellationToken)
    {
        using InThreadJobPool jobPool = new();
        Task task = Task.Run(() => ActionRunner.TryExecute(connectionParameters.MaxAttempts,
            () => GetFileList(connectionParameters, providerParameters,
                (fv, fr) => jobPool.ExecuteAsync(() => pushFileValue(fv, fr)),
                cancellationToken)),
            cancellationToken);
        jobPool.Listen(task);
    }

    public override IFileValue Provide(JsonNode? fileSpecific)
    {
        var data = JsonSerializer.Deserialize<FileSpecificData>(fileSpecific)
            ?? throw new Exception("Invalid file specific");
        return new GraphApiOneDriveFileValue(connectionParameters, data.DriveId, data.DriveItemId, data.FileName, data.FolderPath);
    }

    private void GetFileList(GraphApiAdapterConnectionParameters connectionParameters,
        GraphApiOneDriveAdapterProviderParameters providerParameters,
        Action<GraphApiOneDriveFileValue, FileReference> pushFileValue,
        CancellationToken cancellationToken)
    {
        using var graphClient = connectionParameters.CreateGraphApiClient();

        string driveId, startFolderId;
        var folderDisplayPath = providerParameters.FolderPath?.Trim('/') ?? "";

        if (!string.IsNullOrWhiteSpace(providerParameters.SharingUrl))
        {
            (driveId, startFolderId) = GraphApiOneDriveHelper.ResolveSharingUrl(graphClient, providerParameters.SharingUrl);
            if (!string.IsNullOrEmpty(folderDisplayPath))
                startFolderId = ResolveSubItemIdByPath(graphClient, driveId, startFolderId, folderDisplayPath);
        }
        else
        {
            driveId = GraphApiOneDriveHelper.ResolveDriveId(graphClient, connectionParameters.UserId,
                providerParameters.DriveId, providerParameters.SiteId, providerParameters.SiteUrl,
                providerParameters.GroupId, providerParameters.GroupDisplayNameOrEmail);
            startFolderId = "root";
        }

        var matcher = string.IsNullOrWhiteSpace(providerParameters.FileNamePattern)
            ? null
            : new Matcher().AddInclude(providerParameters.FileNamePattern);

        EnumerateFolder(graphClient, driveId, startFolderId,
            folderDisplayPath, matcher, providerParameters.Recursive, pushFileValue, cancellationToken);
    }

    private void EnumerateFolder(GraphServiceClient graphClient, string driveId, string folderId,
        string folderDisplayPath, Matcher? matcher, bool recursive,
        Action<GraphApiOneDriveFileValue, FileReference> pushFileValue,
        CancellationToken cancellationToken)
    {
        if (folderId == "root" && !string.IsNullOrEmpty(folderDisplayPath))
            folderId = ResolveItemIdByPath(graphClient, driveId, folderDisplayPath);

        var subFolders = new List<(string id, string displayPath)>();
        var page = graphClient.Drives[driveId].Items[folderId].Children
            .GetAsync(null, cancellationToken).GetAwaiter().GetResult();

        if (page == null) return;

        var pageIterator = PageIterator<DriveItem, DriveItemCollectionResponse>.CreatePageIterator(
            graphClient,
            page,
            (item) =>
            {
                if (cancellationToken.IsCancellationRequested) return false;

                if (item.File != null)
                {
                    var fileName = item.Name ?? "";
                    if (matcher == null || matcher.Match(fileName).HasMatches)
                    {
                        var fileValue = new GraphApiOneDriveFileValue(
                            connectionParameters,
                            driveId,
                            item.Id ?? throw new Exception("Drive item id is null"),
                            fileName,
                            folderDisplayPath);
                        var fileReference = new FileReference(fileName, this.Code,
                            JsonSerializer.SerializeToNode(new FileSpecificData
                            {
                                DriveId = driveId,
                                DriveItemId = item.Id!,
                                FileName = fileName,
                                FolderPath = folderDisplayPath
                            }));
                        pushFileValue(fileValue, fileReference);
                    }
                }
                else if (item.Folder != null && recursive && item.Name != null)
                {
                    var subPath = string.IsNullOrEmpty(folderDisplayPath)
                        ? item.Name
                        : $"{folderDisplayPath}/{item.Name}";
                    subFolders.Add((item.Id!, subPath));
                }
                return true;
            });

        pageIterator.IterateAsync(cancellationToken).GetAwaiter().GetResult();

        foreach (var (subId, subPath) in subFolders)
        {
            if (cancellationToken.IsCancellationRequested) break;
            EnumerateFolder(graphClient, driveId, subId, subPath, matcher, recursive, pushFileValue, cancellationToken);
        }
    }

    private static string ResolveItemIdByPath(GraphServiceClient graphClient, string driveId, string path)
    {
        var encodedPath = GraphApiOneDriveHelper.EncodeItemPath(path);
        var url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/root:/{encodedPath}";
        var item = graphClient.Drives[driveId].Root.WithUrl(url).GetAsync().GetAwaiter().GetResult()
            ?? throw new Exception($"Folder not found: {path}");
        return item.Id ?? throw new Exception($"Folder has no id: {path}");
    }

    private static string ResolveSubItemIdByPath(GraphServiceClient graphClient, string driveId, string parentItemId, string relativePath)
    {
        var encodedPath = GraphApiOneDriveHelper.EncodeItemPath(relativePath);
        var url = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{parentItemId}:/{encodedPath}";
        var item = graphClient.Drives[driveId].Root.WithUrl(url).GetAsync().GetAwaiter().GetResult()
            ?? throw new Exception($"Sub-folder not found: {relativePath}");
        return item.Id ?? throw new Exception($"Sub-folder has no id: {relativePath}");
    }

    protected override void Test(GraphApiAdapterConnectionParameters connectionParameters,
        GraphApiOneDriveAdapterProviderParameters providerParameters)
    {
        using var graphClient = connectionParameters.CreateGraphApiClient();
        if (!string.IsNullOrWhiteSpace(providerParameters.SharingUrl))
            GraphApiOneDriveHelper.ResolveSharingUrl(graphClient, providerParameters.SharingUrl);
        else
            GraphApiOneDriveHelper.ResolveDriveId(graphClient, connectionParameters.UserId,
                providerParameters.DriveId, providerParameters.SiteId, providerParameters.SiteUrl,
                providerParameters.GroupId, providerParameters.GroupDisplayNameOrEmail);
    }
}
