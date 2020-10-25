namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
    public class LinkSummary
    {
        public string Name { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public LinkType Type { get; set; }
        public bool Required { get; set; }
    }
}