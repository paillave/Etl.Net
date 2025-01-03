namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
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
    }
}