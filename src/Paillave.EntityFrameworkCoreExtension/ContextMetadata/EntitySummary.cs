using System.Collections.Generic;

namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
    public class EntitySummary : EntitySummaryBase
    {
        public bool IsAbstract { get; set; }
        public bool IsView { get; set; }
        public string Schema { get; set; }
        public List<PropertySummary> Properties { get; set; }
        public string Comment { get; internal set; }

        public override string ToString() => $"{this.Name} ({this.Properties.Count} Properties)";
    }
}