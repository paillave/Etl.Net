using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.MailFolders;
using Paillave.Etl.GraphApi.Provider;

namespace Paillave.Etl.GraphApi;

public static class BaseGraphServiceClientEx
{
    public static async Task<MailFolder> GetFolderAsync(this BaseGraphServiceClient graphClient, string userId, string path)
    {
        var segments = path.Split(new char[] { '/', '\\' });
        var folder = await GetRootFolderAsync(graphClient, userId, segments[0]);
        if (folder == null)
            throw new Exception("folder does not exist");
        foreach (var segment in segments.Skip(1))
        {
            var tmp = await graphClient.Users[userId].MailFolders[folder.Id]
                .GetAsync(i => i.QueryParameters.Expand = ProjectionProcessor<MailFolder>.ToString(j => j.ChildFolders));
            if (tmp == null)
                throw new Exception("this is quiet unexpected");
            folder = tmp.ChildFolders?.FirstOrDefault(i => i.DisplayName == segment);
            if (folder == null)
                throw new Exception("folder does not exist");
        }
        return folder;
    }

    public static async Task<MailFolder?> GetRootFolderAsync(this BaseGraphServiceClient graphClient, string userId, string rootFolderName)
    {
        MailFoldersRequestBuilder mailFoldersRequestBuilder = graphClient.Users[userId].MailFolders;
        var response = await mailFoldersRequestBuilder.GetAsync(i =>
        {
            i.QueryParameters.Filter = ODataExpression<MailFolder>.ToString(j => j.DisplayName == rootFolderName);
            i.QueryParameters.Expand = ProjectionProcessor<MailFolder>.ToString(j => j.ChildFolders);
            i.QueryParameters.Top = 1;
        });
        return response.Value.FirstOrDefault();
    }
}