namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
    public class LinkSummary
    {
        public string Name { get; set; }
        public string FromSchema { get; set; }
        public string FromName { get; set; }
        public string From { get; set; }
        public string ToSchema { get; set; }
        public string ToName { get; set; }
        public string To { get; set; }
        public LinkType Type { get; set; }
        public bool Required { get; set; }
    }
}