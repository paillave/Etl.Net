using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.MailFolders;
using Paillave.Etl.GraphApi.Provider;

namespace Paillave.Etl.GraphApi;

public static class BaseGraphServiceClientEx
{
    public static async Task<MailFolder> GetMailFolderAsync(this BaseGraphServiceClient graphClient, string userId, string path, CancellationToken cancellationToken)
    {
        var segments = path.Split(new char[] { '/', '\\' });
        var folder = await GetMailRootFolderAsync(graphClient, userId, segments[0], cancellationToken);
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

    public static async Task<MailFolder?> GetMailRootFolderAsync(this BaseGraphServiceClient graphClient, string userId, string rootFolderName, CancellationToken cancellationToken)
    {
        MailFoldersRequestBuilder mailFoldersRequestBuilder = graphClient.Users[userId].MailFolders;
        var response = await mailFoldersRequestBuilder.GetAsync(i =>
        {
            i.QueryParameters.Filter = ODataExpression<MailFolder>.ToString(j => j.DisplayName == rootFolderName);
            i.QueryParameters.Expand = ProjectionProcessor<MailFolder>.ToString(j => j.ChildFolders);
            i.QueryParameters.Top = 1;
        }, cancellationToken);
        return response.Value.FirstOrDefault();
    }
    // public static async Task<MailFolder?> GetOneDriveRootFolderAsync(this BaseGraphServiceClient graphClient, string rootFolderName, CancellationToken cancellationToken)
    // {
    //     var mailFoldersRequestBuilder = graphClient.Sites[""].Drives[""];
    //     var response = await mailFoldersRequestBuilder.GetAsync(i =>
    //     {
    //         i.QueryParameters.Filter = ODataExpression<MailFolder>.ToString(j => j.DisplayName == rootFolderName);
    //         i.QueryParameters.Expand = ProjectionProcessor<MailFolder>.ToString(j => j.ChildFolders);
    //         i.QueryParameters.Top = 1;
    //     }, cancellationToken);
    //     return response.Value.FirstOrDefault();
    // }
}