using System.Collections.Generic;

namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata;

public class LinkSummary
{
    public string? Name { get; set; }
    public string? FromSchema { get; set; }
    public required string FromName { get; set; }
    public required string From { get; set; }
    public string? ToSchema { get; set; }
    public required string ToName { get; set; }
    public required string To { get; set; }
    public required LinkType Type { get; set; }
    public required bool Required { get; set; }

    /// <summary>True when EF Core is configured with <c>DeleteBehavior.Cascade</c> for the underlying
    /// foreign key: deleting a principal-side row also deletes its dependent-side row(s). Always false
    /// for a <see cref="LinkType.Inherits"/> link, which isn't backed by a foreign key.</summary>
    public required bool CascadeDelete { get; set; }

    /// <summary>The CLR name(s) of the foreign-key column(s) that implement this relationship. These are
    /// always declared on the dependent/child entity — for a <see cref="LinkType.Aggregates"/> link seen
    /// from the principal/parent side, they are property names on <see cref="ToName"/>, not on
    /// <see cref="FromName"/>. Empty for a <see cref="LinkType.Inherits"/> link or a many-to-many
    /// (skip-navigation) <see cref="LinkType.Aggregates"/> link, neither of which is backed by a single
    /// foreign key on either side.</summary>
    public required IReadOnlyList<string> ForeignKeyProperties { get; set; }
}