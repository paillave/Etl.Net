namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
    public class PropertySummary
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public required bool IsNullable { get; set; }
        public required bool IsKey { get; set; }
        public required bool IsForeignKey { get; set; }
        public int? MaxLength { get; set; }
        public override string ToString() => $"{this.Name}:{this.Type}";
    }
}