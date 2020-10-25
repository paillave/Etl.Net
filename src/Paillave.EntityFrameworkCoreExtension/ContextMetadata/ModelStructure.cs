using System.Collections.Generic;

namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
    public class ModelStructure
    {
        public Dictionary<string, EntitySummary> Entities { get; set; }
        public List<LinkSummary> Links { get; set; }
        public Dictionary<string, string> Comments { get; set; }
    }
}