using System.Collections.Generic;

namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata;

public class ModelStructure
{
    public required Dictionary<string, EntitySummary> Entities { get; set; }
    public required List<LinkSummary> Links { get; set; }
    public Dictionary<string, string>? Comments { get; set; }
}