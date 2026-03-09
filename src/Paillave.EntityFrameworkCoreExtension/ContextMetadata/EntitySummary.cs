using System.Collections.Generic;

namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata;

public class EntitySummary : EntitySummaryBase
{
    public required bool IsAbstract { get; set; }
    public required bool IsView { get; set; }
    public string? Schema { get; set; }
    public required List<PropertySummary> Properties { get; set; }
    public string? Comment { get; set; }

    public override string ToString() => $"{this.Name} ({this.Properties.Count} Properties)";
}