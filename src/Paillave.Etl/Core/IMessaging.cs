using System.IO;
using System.Collections.Generic;
using System;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace Paillave.Etl.Core;

public interface IMessaging
{
    string Name { get; }

    void Send(MessageContact? sender, string subject, string body, bool important, MessageContact[] entities, Dictionary<string, Stream>? attachments = null);
}
public class MessageContact
{
    public string? DisplayName { get; set; }
    public required string Email { get; set; }
}
public interface IMessagingProvider
{
    string Name { get; }
    IMessaging GetMessaging(JsonNode configuration);
}

public abstract class MessagingProviderBase<TConfiguration> : IMessagingProvider where TConfiguration : class
{
    public abstract string Name { get; }
    public abstract IMessaging GetMessaging(TConfiguration configuration);
    IMessaging IMessagingProvider.GetMessaging(JsonNode configuration)
        => GetMessaging(JsonSerializer.Deserialize<TConfiguration>(configuration) ??
            throw new InvalidOperationException($"Invalid configuration for messaging provider '{this.Name}'"));
}