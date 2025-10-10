using System;
using System.Threading;
using Paillave.Etl.Core;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Paillave.Etl.GraphApi.Requesting;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.Graph.Models;
using System.Text.Json;

namespace Paillave.Etl.GraphApi;

public class GraphApiMailAdapterProviderParameters
{
    public bool? OnlyNotRead { get; set; }
    public string FromContains { get; set; }
    public string SubjectContains { get; set; }
    public string AttachmentNamePattern { get; set; }
    [Required]
    public string Folder { get; set; }
    public bool SetToReadIfBatchDeletion { get; set; }
}
public partial class GraphApiMailFileValueProvider(string code, string name, string connectionName, GraphApiAdapterConnectionParameters connectionParameters, GraphApiMailAdapterProviderParameters providerParameters) : FileValueProviderBase<GraphApiAdapterConnectionParameters, GraphApiMailAdapterProviderParameters>(code, name, connectionName, connectionParameters, providerParameters)
{
    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
    protected override void Provide(object input, Action<IFileValue, FileReference> pushFileValue, GraphApiAdapterConnectionParameters connectionParameters, GraphApiMailAdapterProviderParameters providerParameters, CancellationToken cancellationToken)
    {
        using InThreadJobPool jobPool = new InThreadJobPool();
        Task task = Task.Run(() => ActionRunner.TryExecute(connectionParameters.MaxAttempts, () => GetFileList(connectionParameters, providerParameters, (fv, fr) => jobPool.ExecuteAsync(() => pushFileValue(fv, fr)), cancellationToken)));
        jobPool.Listen(task);
    }
    private class FileSpecificData
    {
        public required string MessageId { get; set; }
        public required string AttachmentId { get; set; }
        public required string Subject { get; set; }
        public required string SenderAddress { get; set; }
        public required string SenderName { get; set; }
        public required string AttachmentName { get; set; }
        public required DateTime ReceivedDateTime { get; set; }
        public required Dictionary<string, bool> DeletionDico { get; set; }
    }

    public override IFileValue Provide(string fileSpecific)
    {
        var fileSpecificData = JsonSerializer.Deserialize<FileSpecificData>(fileSpecific) ?? throw new Exception("Invalid file specific");
        return new GraphApiMailFileValue(connectionParameters, fileSpecificData.MessageId, fileSpecificData.AttachmentId,
            fileSpecificData.AttachmentName, providerParameters.SetToReadIfBatchDeletion,
            fileSpecificData.DeletionDico, fileSpecificData.Subject, fileSpecificData.SenderAddress,
            fileSpecificData.SenderName, fileSpecificData.ReceivedDateTime);
    }
    private void GetFileList(GraphApiAdapterConnectionParameters connectionParameters, GraphApiMailAdapterProviderParameters providerParameters,
        Action<GraphApiMailFileValue, FileReference> pushFileValue, CancellationToken cancellationToken)
    {
        // List<GraphApiMailFileValue> fileValues = new List<GraphApiMailFileValue>();
        using var graphClient = connectionParameters.CreateGraphApiClient();
        var inputFolder = graphClient.GetFolderAsync(connectionParameters.UserId, providerParameters.Folder, cancellationToken).Result;

        var matcher = string.IsNullOrWhiteSpace(providerParameters.AttachmentNamePattern) 
            ? null 
            : new Matcher().AddInclude(providerParameters.AttachmentNamePattern);

        graphClient.Users[connectionParameters.UserId].MailFolders[inputFolder.Id].Messages
            .Where(CreateQuery(providerParameters))
            .Select(obj => new { obj.Id, obj.ParentFolderId, obj.ReceivedDateTime, obj.From })
            .Expand(obj => obj.Attachments)
            .FetchAsync(graphClient, message =>
            {
                var fullMessageRequestBuilder = graphClient.Users[connectionParameters.UserId].MailFolders[message.ParentFolderId].Messages[message.Id];
                var attachments = new List<Attachment>();
                fullMessageRequestBuilder.Attachments
                    .Select(j => new { j.Name, j.Id })
                    .FetchAsync(graphClient, a =>
                    {
                        attachments.Add(a);
                        return Task.FromResult(true);
                    }).Wait();
                var deletionDico = attachments.ToDictionary(i => i.Id 
                    ?? throw new Exception("Message id is null"), i => false);
                foreach (var item in attachments)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    if (matcher == null || matcher.Match(item.Name ?? "").HasMatches)
                    {
                        var fileValue = new GraphApiMailFileValue(
                            connectionParameters,
                            message.Id ?? throw new Exception("Message id is null"),
                            item.Id ?? throw new Exception("Attachment id is null"),
                            item.Name ?? "",
                            providerParameters.SetToReadIfBatchDeletion,
                            deletionDico,
                            message.Subject ?? "",
                            message.From?.EmailAddress?.Address ?? "",
                            message.From?.EmailAddress?.Name ?? "",
                            message.ReceivedDateTime.Value.UtcDateTime
                        );
                        var fileReference = new FileReference(fileValue.Name, this.Code, JsonSerializer.Serialize(new FileSpecificData
                        {
                            AttachmentId = item.Id,
                            MessageId = message.Id,
                            Subject = message.Subject ?? "",
                            AttachmentName = item.Name ?? "",
                            SenderAddress = message.From?.EmailAddress?.Address ?? "",
                            ReceivedDateTime = message.ReceivedDateTime.Value.UtcDateTime,
                            DeletionDico = deletionDico,
                            SenderName = message.From?.EmailAddress?.Name ?? "",
                        }));
                        pushFileValue(fileValue, fileReference);
                        // fileValues.Add(fileValue);
                    }
                }
                return Task.FromResult(true);
            }, cancellationToken).Wait();
        // return fileValues;
    }
    private Expression<Func<Message, bool>> CreateQuery(GraphApiMailAdapterProviderParameters providerParameters)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(Message), "message");

        MemberExpression hasAttachmentsProperty = Expression.Property(parameter, "HasAttachments");
        ConstantExpression trueConstant = Expression.Constant(true, typeof(bool?));
        BinaryExpression condition = Expression.Equal(hasAttachmentsProperty, trueConstant);

        if (!string.IsNullOrWhiteSpace(providerParameters.FromContains))
        {
            ConstantExpression fundConstant = Expression.Constant(providerParameters.FromContains);

            MemberExpression fromProperty = Expression.Property(parameter, "From");
            MemberExpression emailProperty = Expression.Property(fromProperty, "EmailAddress");
            MemberExpression addressProperty = Expression.Property(emailProperty, "Address");
            MethodCallExpression containsCall = Expression.Call(addressProperty, "Contains", null, fundConstant);

            MemberExpression addressProperty1 = Expression.Property(emailProperty, "Name");
            MethodCallExpression containsCall1 = Expression.Call(addressProperty, "Contains", null, fundConstant);

            condition = Expression.AndAlso(condition, Expression.OrElse(containsCall, containsCall1));
        }

        if (!string.IsNullOrWhiteSpace(providerParameters.SubjectContains))
        {
            ConstantExpression fundConstant = Expression.Constant(providerParameters.FromContains);

            MemberExpression subjectProperty = Expression.Property(parameter, "Subject");
            MethodCallExpression containsCall = Expression.Call(subjectProperty, "Contains", null, fundConstant);

            condition = Expression.AndAlso(condition, containsCall);
        }
        if (providerParameters.OnlyNotRead.HasValue && providerParameters.OnlyNotRead.Value)
        {
            MemberExpression isReadProperty = Expression.Property(parameter, "IsRead");
            ConstantExpression falseConstant = Expression.Constant(false, typeof(bool?));
            BinaryExpression isReadOnlyCondition = Expression.Equal(isReadProperty, falseConstant);
            condition = Expression.AndAlso(condition, isReadOnlyCondition);
        }

        return Expression.Lambda<Func<Message, bool>>(condition, parameter);
    }
    protected override void Test(GraphApiAdapterConnectionParameters connectionParameters, GraphApiMailAdapterProviderParameters providerParameters)
    {
        using var client = connectionParameters.CreateGraphApiClient();
        client.Users[connectionParameters.UserId].MailFolders.GetAsync().Wait();
    }
}
