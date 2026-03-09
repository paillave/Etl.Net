using System.IO;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Configuration;

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
    IMessaging GetMessaging(IConfigurationSection configurationSection);
}

public abstract class MessagingProviderBase<TConfiguration> : IMessagingProvider where TConfiguration : class
{
    public abstract string Name { get; }
    public abstract IMessaging GetMessaging(TConfiguration configuration);
    IMessaging IMessagingProvider.GetMessaging(IConfigurationSection parameters)
        => GetMessaging(parameters.Get<TConfiguration>() ?? Activator.CreateInstance<TConfiguration>());
}