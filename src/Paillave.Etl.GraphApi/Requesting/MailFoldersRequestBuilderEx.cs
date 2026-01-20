using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Linq.Expressions;
using Microsoft.Graph.Users.Item.MailFolders;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System;
using Paillave.Etl.GraphApi.Provider;

namespace Paillave.Etl.GraphApi.Requesting;
public static class MailFoldersRequestBuilderEx
{
    public static MailFoldersRequestBuilderGetBuilder Where(this MailFoldersRequestBuilder builder, Expression<Func<MailFolder, bool>> filter)
    {
        return new MailFoldersRequestBuilderGetBuilder
        {
            Filter = ODataExpression<MailFolder>.ToString(filter),
            MailFoldersRequestBuilder = builder
        };
    }
    public static MailFoldersRequestBuilderGetBuilder Select<TRes>(this MailFoldersRequestBuilder builder, Expression<Func<MailFolder, TRes>> fields)
    {
        return new MailFoldersRequestBuilderGetBuilder
        {
            Select = ProjectionProcessor<MailFolder>.ToString(fields),
            MailFoldersRequestBuilder = builder
        };
    }
    public static MailFoldersRequestBuilderGetBuilder Select<TRes>(this MailFoldersRequestBuilderGetBuilder builder, Expression<Func<MailFolder, TRes>> fields)
    {
        builder.Select = ProjectionProcessor<MailFolder>.ToString(fields);
        return builder;
    }
    public static async Task FetchAsync(this MailFoldersRequestBuilderGetBuilder builder, IBaseClient graphClient, Func<MailFolder, Task<bool>> processFolder, CancellationToken? cancellationToken = null)
    {
        var withAttachmentFoldersPage = await builder.MailFoldersRequestBuilder.GetAsync(i =>
            {
                i.QueryParameters.Filter = builder.Filter;
                i.QueryParameters.Select = builder.Select;
            });

        var pageIterator = Microsoft.Graph.PageIterator<MailFolder, MailFolderCollectionResponse>.CreatePageIterator(graphClient, withAttachmentFoldersPage, processFolder);

        await pageIterator.IterateAsync(cancellationToken ?? CancellationToken.None);
    }
    public static Task<List<MailFolder>> SelectAsync(this MailFoldersRequestBuilderGetBuilder builder, IBaseClient graphClient, CancellationToken? cancellationToken = null)
        => builder.SelectAsync(graphClient, i => i, cancellationToken);
    public static async Task<List<TOut>> SelectAsync<TOut>(this MailFoldersRequestBuilderGetBuilder builder, IBaseClient graphClient, Func<MailFolder, TOut> selectFolder, CancellationToken? cancellationToken = null)
    {
        var mailFoldersRequestBuilder = builder.MailFoldersRequestBuilder;
        var withAttachmentFoldersPage = await mailFoldersRequestBuilder.GetAsync(i =>
            {
                i.QueryParameters.Filter = builder.Filter;
                i.QueryParameters.Select = builder.Select;
            });
        return await IterateAsync(graphClient, selectFolder, cancellationToken, withAttachmentFoldersPage);
    }
    public static async Task<List<TOut>> SelectAsync<TOut>(this MailFoldersRequestBuilder builder, IBaseClient graphClient, Func<MailFolder, TOut> selectFolder, CancellationToken? cancellationToken = null)
    {
        var mailFoldersRequestBuilder = builder;
        var withAttachmentFoldersPage = await mailFoldersRequestBuilder.GetAsync();
        return await IterateAsync(graphClient, selectFolder, cancellationToken, withAttachmentFoldersPage);
    }
    public static async Task<List<MailFolder>> SelectAsync(this MailFoldersRequestBuilder builder, IBaseClient graphClient, CancellationToken? cancellationToken = null)
    {
        var mailFoldersRequestBuilder = builder;
        var withAttachmentFoldersPage = await mailFoldersRequestBuilder.GetAsync();
        return await IterateAsync(graphClient, i => i, cancellationToken, withAttachmentFoldersPage);
    }

    private static async Task<List<TOut>> IterateAsync<TOut>(IBaseClient graphClient, Func<MailFolder, TOut> selectFolder, CancellationToken? cancellationToken, MailFolderCollectionResponse? withAttachmentFoldersPage)
    {
        List<TOut> output = new();
        var pageIterator = Microsoft.Graph.PageIterator<MailFolder, MailFolderCollectionResponse>.CreatePageIterator(graphClient, withAttachmentFoldersPage, m =>
        {
            output.Add(selectFolder(m));
            return true;
        });

        await pageIterator.IterateAsync(cancellationToken ?? CancellationToken.None);
        return output;
    }
}
