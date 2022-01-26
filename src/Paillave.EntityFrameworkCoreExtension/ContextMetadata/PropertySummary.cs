namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
    public class PropertySummary
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsNullable { get; set; }
        public bool IsKey { get; set; }
        public bool IsForeignKey { get; set; }
        public int? MaxLength { get; set; }
        public override string ToString() => $"{this.Name}:{this.Type}";
    }
}