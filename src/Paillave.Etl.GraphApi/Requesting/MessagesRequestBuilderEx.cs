using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.MailFolders.Item.Messages;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Paillave.Etl.GraphApi.Provider;
using System.Linq;
using System.Threading;

namespace Paillave.Etl.GraphApi.Requesting;

public static class MessagesRequestBuilderEx
{
    public static MessagesRequestBuilderGetBuilder Where(this MessagesRequestBuilder builder, Expression<Func<Message, bool>> filter)
    {        return new MessagesRequestBuilderGetBuilder
        {
            Filter = ODataExpression<Message>.ToString(filter),
            MessagesRequestBuilder = builder
        };
    }
    public static MessagesRequestBuilderGetBuilder Select<TRes>(this MessagesRequestBuilder builder, Expression<Func<Message, TRes>> fields)
    {
        return new MessagesRequestBuilderGetBuilder
        {
            Select = ProjectionProcessor<Message>.ToString(fields),
            MessagesRequestBuilder = builder
        };
    }
    public static MessagesRequestBuilderGetBuilder Select<TRes>(this MessagesRequestBuilderGetBuilder builder, Expression<Func<Message, TRes>> fields)
    {
        builder.Select = ProjectionProcessor<Message>.ToString(fields);
        return builder;
    }
    public static MessagesRequestBuilderGetBuilder Expand<TRes>(this MessagesRequestBuilderGetBuilder builder, Expression<Func<Message, TRes>> fields)
    {
        builder.Expand = ProjectionProcessor<Message>.ToString(fields);
        return builder;
    }
    public static async Task FetchAsync(this MessagesRequestBuilderGetBuilder builder, IBaseClient graphClient, Func<Message, Task<bool>> processMessage, CancellationToken? cancellationToken = null)
    {
        var withAttachmentMessagesPage = await builder.MessagesRequestBuilder.GetAsync(i =>
            {
                i.QueryParameters.Filter = builder.Filter;
                i.QueryParameters.Select = builder.Select;
                i.QueryParameters.Expand = builder.Expand;
            });

        var pageIterator = Microsoft.Graph.PageIterator<Message, MessageCollectionResponse>.CreatePageIterator(graphClient, withAttachmentMessagesPage, processMessage);

        await pageIterator.IterateAsync(cancellationToken ?? CancellationToken.None);
    }
    // public static async Task FetchAsync(this Microsoft.Graph.Users.Item.MailFolders.Item.Messages.Item.Attachments.AttachmentsRequestBuilder builder, IBaseClient graphClient, Func<Attachment, Task<bool>> processMessage, CancellationToken? cancellationToken = null)
    // {
    //     var withAttachmentMessagesPage = await builder.GetAsync(i =>
    //         {
    //             i.QueryParameters.Filter = builder.Filter;
    //             i.QueryParameters.Select = builder.Select;
    //             i.QueryParameters.Expand = builder.Expand;
    //         });

    //     var pageIterator = Microsoft.Graph.PageIterator<Message, MessageCollectionResponse>.CreatePageIterator(graphClient, withAttachmentMessagesPage, processMessage);

    //     await pageIterator.IterateAsync(cancellationToken ?? CancellationToken.None);
    // }
    public static Task<List<Message>> SelectAsync(this MessagesRequestBuilderGetBuilder builder, IBaseClient graphClient, CancellationToken? cancellationToken = null)
        => builder.SelectAsync(graphClient, i => i, cancellationToken);
    public static async Task<List<TOut>> SelectAsync<TOut>(this MessagesRequestBuilderGetBuilder builder, IBaseClient graphClient, Func<Message, TOut> selectMessage, CancellationToken? cancellationToken = null)
    {
        var withAttachmentMessagesPage = await builder.MessagesRequestBuilder.GetAsync(i =>
            {
                i.QueryParameters.Filter = builder.Filter;
                i.QueryParameters.Select = builder.Select;
            });
        List<TOut> output = new();
        var pageIterator = Microsoft.Graph.PageIterator<Message, MessageCollectionResponse>.CreatePageIterator(graphClient, withAttachmentMessagesPage, m =>
        {
            output.Add(selectMessage(m));
            return true;
        });

        await pageIterator.IterateAsync(cancellationToken ?? CancellationToken.None);
        return output;
    }
}
