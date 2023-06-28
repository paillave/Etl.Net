using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.MailFolders.Item.Messages.Item.Attachments;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Paillave.Etl.GraphApi.Provider;
using System.Linq;
using System.Threading;

namespace Paillave.Etl.GraphApi.Requesting;

public static class AttachmentsRequestBuilderEx
{
    public static AttachmentsRequestBuilderGetBuilder Where(this AttachmentsRequestBuilder builder, Expression<Func<Attachment, bool>> filter)
    {
        return new AttachmentsRequestBuilderGetBuilder
        {
            Filter = ODataExpression<Attachment>.ToString(filter),
            AttachmentsRequestBuilder = builder
        };
    }
    public static AttachmentsRequestBuilderGetBuilder Select<TRes>(this AttachmentsRequestBuilder builder, Expression<Func<Attachment, TRes>> fields)
    {
        return new AttachmentsRequestBuilderGetBuilder
        {
            Select = ProjectionProcessor<Attachment>.ToString(fields),
            AttachmentsRequestBuilder = builder
        };
    }
    public static AttachmentsRequestBuilderGetBuilder Select<TRes>(this AttachmentsRequestBuilderGetBuilder builder, Expression<Func<Attachment, TRes>> fields)
    {
        builder.Select = ProjectionProcessor<Attachment>.ToString(fields);
        return builder;
    }
    public static AttachmentsRequestBuilderGetBuilder Expand<TRes>(this AttachmentsRequestBuilderGetBuilder builder, Expression<Func<Attachment, TRes>> fields)
    {
        builder.Expand = ProjectionProcessor<Attachment>.ToString(fields);
        return builder;
    }
    public static async Task FetchAsync(this AttachmentsRequestBuilderGetBuilder builder, IBaseClient graphClient, Func<Attachment, Task<bool>> processAttachment, CancellationToken? cancellationToken = null)
    {
        var withAttachmentAttachmentsPage = await builder.AttachmentsRequestBuilder.GetAsync(i =>
            {
                i.QueryParameters.Filter = builder.Filter;
                i.QueryParameters.Select = builder.Select;
                i.QueryParameters.Expand = builder.Expand;
            });

        var pageIterator = Microsoft.Graph.PageIterator<Attachment, AttachmentCollectionResponse>.CreatePageIterator(graphClient, withAttachmentAttachmentsPage, processAttachment);

        await pageIterator.IterateAsync(cancellationToken ?? CancellationToken.None);
    }
    // public static async Task FetchAsync(this Microsoft.Graph.Users.Item.MailFolders.Item.Attachments.Item.Attachments.AttachmentsRequestBuilder builder, IBaseClient graphClient, Func<Attachment, Task<bool>> processAttachment, CancellationToken? cancellationToken = null)
    // {
    //     var withAttachmentAttachmentsPage = await builder.GetAsync(i =>
    //         {
    //             i.QueryParameters.Filter = builder.Filter;
    //             i.QueryParameters.Select = builder.Select;
    //             i.QueryParameters.Expand = builder.Expand;
    //         });

    //     var pageIterator = Microsoft.Graph.PageIterator<Attachment, AttachmentCollectionResponse>.CreatePageIterator(graphClient, withAttachmentAttachmentsPage, processAttachment);

    //     await pageIterator.IterateAsync(cancellationToken ?? CancellationToken.None);
    // }
    public static Task<List<Attachment>> SelectAsync(this AttachmentsRequestBuilderGetBuilder builder, IBaseClient graphClient, CancellationToken? cancellationToken = null)
        => builder.SelectAsync(graphClient, i => i, cancellationToken);
    public static async Task<List<TOut>> SelectAsync<TOut>(this AttachmentsRequestBuilderGetBuilder builder, IBaseClient graphClient, Func<Attachment, TOut> selectAttachment, CancellationToken? cancellationToken = null)
    {
        var withAttachmentAttachmentsPage = await builder.AttachmentsRequestBuilder.GetAsync(i =>
            {
                i.QueryParameters.Filter = builder.Filter;
                i.QueryParameters.Select = builder.Select;
            });
        List<TOut> output = new List<TOut>();
        var pageIterator = Microsoft.Graph.PageIterator<Attachment, AttachmentCollectionResponse>.CreatePageIterator(graphClient, withAttachmentAttachmentsPage, m =>
        {
            output.Add(selectAttachment(m));
            return true;
        });

        await pageIterator.IterateAsync(cancellationToken ?? CancellationToken.None);
        return output;
    }
}
