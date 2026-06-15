using Microsoft.Graph;
using System;
using System.Linq;
using System.Text;

namespace Paillave.Etl.GraphApi;

internal static class GraphApiOneDriveHelper
{
    internal static string EncodeItemPath(string path)
    {
        var segments = path.Split('/');
        var encoded = new string[segments.Length];
        for (int i = 0; i < segments.Length; i++)
            encoded[i] = Uri.EscapeDataString(segments[i]);
        return string.Join("/", encoded);
    }

    internal static (string DriveId, string FolderItemId) ResolveSharingUrl(
        GraphServiceClient graphClient, string sharingUrl)
    {
        // Navigation URL (browser address bar): ?id=/path/to/folder&listurl=https://...
        if (TryParseNavigationUrl(sharingUrl, out var siteUrl, out var folderPath))
        {
            var driveId = ResolveDriveId(graphClient, null, null, null, siteUrl, null, null);
            if (string.IsNullOrEmpty(folderPath))
                return (driveId, "root");
            var encodedPath = EncodeItemPath(folderPath);
            var folderUrl = $"https://graph.microsoft.com/v1.0/drives/{driveId}/root:/{encodedPath}";
            var folderItem = graphClient.Drives[driveId].Root.WithUrl(folderUrl).GetAsync().GetAwaiter().GetResult()
                ?? throw new Exception($"Folder not found: {folderPath}");
            return (driveId, folderItem.Id ?? throw new Exception($"Folder has no ID: {folderPath}"));
        }

        // Standard Graph API sharing link: "u!" + base64url(url) without padding
        var shareId = "u!" + Convert.ToBase64String(Encoding.UTF8.GetBytes(sharingUrl))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        var item = graphClient.Shares[shareId].DriveItem
            .GetAsync().GetAwaiter().GetResult()
            ?? throw new Exception($"Could not resolve sharing URL: {sharingUrl}");

        // For cross-site sharing the real identity is in RemoteItem
        var driveId2 = item.RemoteItem?.ParentReference?.DriveId
            ?? item.ParentReference?.DriveId
            ?? throw new Exception($"Could not determine drive ID from sharing URL: {sharingUrl}");
        var itemId = item.RemoteItem?.Id
            ?? item.Id
            ?? throw new Exception($"Could not determine item ID from sharing URL: {sharingUrl}");

        return (driveId2, itemId);
    }

    private static bool TryParseNavigationUrl(string url, out string siteUrl, out string folderPath)
    {
        siteUrl = "";
        folderPath = "";
        try
        {
            var uri = new Uri(url);
            var query = uri.Query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Split('=', 2))
                .Where(p => p.Length == 2)
                .ToDictionary(p => Uri.UnescapeDataString(p[0]), p => Uri.UnescapeDataString(p[1]),
                    StringComparer.OrdinalIgnoreCase);

            if (!query.TryGetValue("id", out var id) || !query.TryGetValue("listurl", out var listUrl))
                return false;

            // listurl = "https://tenant.sharepoint.com/sites/Finance/Documents"
            // → site  = "https://tenant.sharepoint.com/sites/Finance"  (strip last segment = library)
            var listUri = new Uri(listUrl);
            var segments = listUri.AbsolutePath.TrimEnd('/').Split('/');
            var sitePath = string.Join("/", segments.Take(segments.Length - 1));
            siteUrl = $"{listUri.Scheme}://{listUri.Host}{sitePath}";

            // id = "/sites/Finance/Documents/TestFromApp"
            // library prefix = "/sites/Finance/Documents"  (= listUri.AbsolutePath without trailing slash)
            // → drive-relative path = "TestFromApp"
            var libraryPath = listUri.AbsolutePath.TrimEnd('/');
            var serverRelative = id.TrimEnd('/');
            folderPath = serverRelative.StartsWith(libraryPath + "/", StringComparison.OrdinalIgnoreCase)
                ? serverRelative[(libraryPath.Length + 1)..]
                : string.Compare(serverRelative, libraryPath, StringComparison.OrdinalIgnoreCase) == 0
                    ? ""          // id points to the library itself → list from drive root
                    : id.TrimStart('/');

            return true;
        }
        catch { return false; }
    }

    internal static string ResolveDriveId(GraphServiceClient graphClient, string? defaultUserId,
        string? driveId, string? siteId, string? siteUrl, string? groupId, string? groupDisplayNameOrEmail)
    {
        if (!string.IsNullOrWhiteSpace(driveId))
            return driveId;

        if (!string.IsNullOrWhiteSpace(siteUrl))
        {
            var uri = new Uri(siteUrl);
            // Graph API accepts "hostname:/server-relative-path" as site identifier
            var siteIdentifier = $"{uri.Host}:{uri.AbsolutePath.TrimEnd('/')}";
            return graphClient.Sites[siteIdentifier].Drive.GetAsync().GetAwaiter().GetResult()?.Id
                ?? throw new Exception($"Could not get drive for site URL: {siteUrl}");
        }

        if (!string.IsNullOrWhiteSpace(siteId))
            return graphClient.Sites[siteId].Drive.GetAsync().GetAwaiter().GetResult()?.Id
                ?? throw new Exception($"Could not get drive for site ID: {siteId}");

        if (!string.IsNullOrWhiteSpace(groupDisplayNameOrEmail))
        {
            var safe = groupDisplayNameOrEmail.Replace("'", "''"); // OData single-quote escape
            var response = graphClient.Groups.GetAsync(r =>
            {
                r.QueryParameters.Filter = $"displayName eq '{safe}' or mail eq '{safe}'";
                r.QueryParameters.Top = 1;
                r.QueryParameters.Select = ["id"];
            }).GetAwaiter().GetResult();
            var group = response?.Value?.FirstOrDefault()
                ?? throw new Exception($"Group/Team not found: {groupDisplayNameOrEmail}");
            return graphClient.Groups[group.Id!].Drive.GetAsync().GetAwaiter().GetResult()?.Id
                ?? throw new Exception($"Could not get drive for group: {groupDisplayNameOrEmail}");
        }

        if (!string.IsNullOrWhiteSpace(groupId))
            return graphClient.Groups[groupId].Drive.GetAsync().GetAwaiter().GetResult()?.Id
                ?? throw new Exception($"Could not get drive for group ID: {groupId}");

        if (string.IsNullOrWhiteSpace(defaultUserId))
            throw new Exception("UserId is required when no DriveId, SiteUrl, SiteId, GroupId or GroupDisplayNameOrEmail is provided.");
        return graphClient.Users[defaultUserId].Drive.GetAsync().GetAwaiter().GetResult()?.Id
            ?? throw new Exception($"Could not get drive for user: {defaultUserId}");
    }
}
