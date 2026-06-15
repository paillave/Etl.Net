using Microsoft.Graph;
using Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession;
using Microsoft.Graph.Models;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Paillave.Etl.GraphApi;

public class GraphApiOneDriveAdapterProcessorParameters
{
    /// <summary>Destination folder path inside the drive (e.g. "Reports/2024"). Leave empty for root.</summary>
    public string? FolderPath { get; set; }
    public bool UseStreamCopy { get; set; } = true;

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
    /// When set, the drive and destination folder are resolved from this link; DriveId/SiteUrl/etc. are ignored.
    /// FolderPath is treated as a sub-path inside the shared folder.
    /// </summary>
    public string? SharingUrl { get; set; }
}

public class GraphApiOneDriveFileValueProcessor(string code, string name, string connectionName,
    GraphApiAdapterConnectionParameters connectionParameters,
    GraphApiOneDriveAdapterProcessorParameters processorParameters)
    : FileValueProcessorBase<GraphApiAdapterConnectionParameters, GraphApiOneDriveAdapterProcessorParameters>(code, name, connectionName, connectionParameters, processorParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

    protected override void Process(IFileValue fileValue,
        GraphApiAdapterConnectionParameters connectionParameters,
        GraphApiOneDriveAdapterProcessorParameters processorParameters,
        Action<IFileValue> push,
        CancellationToken cancellationToken)
    {
        using var stream = fileValue.Get(processorParameters.UseStreamCopy);
        var ms = new MemoryStream();
        stream.CopyTo(ms);

        ActionRunner.TryExecute(connectionParameters.MaxAttempts,
            () => UploadFileSingleTime(connectionParameters, processorParameters, fileValue.Name, ms));

        push(fileValue);
    }

    private static void UploadFileSingleTime(GraphApiAdapterConnectionParameters connectionParameters,
        GraphApiOneDriveAdapterProcessorParameters processorParameters, string fileName, MemoryStream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);
        using var graphClient = connectionParameters.CreateGraphApiClient();

        var folderPath = processorParameters.FolderPath?.Trim('/') ?? "";
        var itemPath = string.IsNullOrEmpty(folderPath) ? fileName : $"{folderPath}/{fileName}";
        var encodedPath = GraphApiOneDriveHelper.EncodeItemPath(itemPath);

        string driveId, sessionUrl;
        if (!string.IsNullOrWhiteSpace(processorParameters.SharingUrl))
        {
            string sharedFolderId;
            (driveId, sharedFolderId) = GraphApiOneDriveHelper.ResolveSharingUrl(graphClient, processorParameters.SharingUrl);
            sessionUrl = $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{sharedFolderId}:/{encodedPath}:/createUploadSession";
        }
        else
        {
            driveId = GraphApiOneDriveHelper.ResolveDriveId(graphClient, connectionParameters.UserId,
                processorParameters.DriveId, processorParameters.SiteId, processorParameters.SiteUrl,
                processorParameters.GroupId, processorParameters.GroupDisplayNameOrEmail);
            sessionUrl = $"https://graph.microsoft.com/v1.0/drives/{driveId}/root:/{encodedPath}:/createUploadSession";
        }

        // Upload session works for any file size (OneDrive requires sessions for files > 4 MB)
        var uploadSessionBody = new CreateUploadSessionPostRequestBody
        {
            Item = new DriveItemUploadableProperties
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "@microsoft.graph.conflictBehavior", "replace" }
                }
            }
        };

        var uploadSession = graphClient.Drives[driveId].Items["placeholder"].CreateUploadSession
            .WithUrl(sessionUrl)
            .PostAsync(uploadSessionBody)
            .GetAwaiter().GetResult()
            ?? throw new Exception("Failed to create OneDrive upload session");

        // 320 KB chunks — must be a multiple of 320 KB per OneDrive spec
        var uploadTask = new LargeFileUploadTask<DriveItem>(uploadSession, ms, 320 * 1024, graphClient.RequestAdapter);
        uploadTask.UploadAsync().GetAwaiter().GetResult();
    }

    protected override void Test(GraphApiAdapterConnectionParameters connectionParameters,
        GraphApiOneDriveAdapterProcessorParameters processorParameters)
    {
        using var graphClient = connectionParameters.CreateGraphApiClient();
        if (!string.IsNullOrWhiteSpace(processorParameters.SharingUrl))
            GraphApiOneDriveHelper.ResolveSharingUrl(graphClient, processorParameters.SharingUrl);
        else
            GraphApiOneDriveHelper.ResolveDriveId(graphClient, connectionParameters.UserId,
                processorParameters.DriveId, processorParameters.SiteId, processorParameters.SiteUrl,
                processorParameters.GroupId, processorParameters.GroupDisplayNameOrEmail);
    }
}
