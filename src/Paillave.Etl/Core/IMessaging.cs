using System.IO;
using System.Collections.Generic;

namespace Paillave.Etl.Core;

public interface IMessaging
{
    string Name { get; }

    void Send(MessageDestination? sender, string subject, string body, bool important, MessageDestination[] entities, Dictionary<string, Stream>? attachments = null);
}
public class MessageDestination
{
    public string? DisplayName { get; set; }
    public required string Email { get; set; }
}
