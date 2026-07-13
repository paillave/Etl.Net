namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata;

public class PropertySummary
{
    public required string Name { get; set; }
    /// <summary>The CLR property name, as declared on the entity class — may differ from <see cref="Name"/>
    /// (the physical SQL column name) when EF Core's shared-table convention renames a column to avoid a
    /// collision, e.g. between two unrelated sibling TPH leaf types that happen to declare a same-named property.</summary>
    public required string ClrName { get; set; }
    public required string Type { get; set; }
    public required bool IsNullable { get; set; }
    public required bool IsKey { get; set; }
    public required bool IsForeignKey { get; set; }
    public int? MaxLength { get; set; }
    public override string ToString() => $"{this.Name}:{this.Type}";
}