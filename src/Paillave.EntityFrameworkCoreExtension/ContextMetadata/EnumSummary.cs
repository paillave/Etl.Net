using System.Collections.Generic;

namespace Paillave.EntityFrameworkCoreExtension.ContextMetadata
{
    public class EnumSummary : EntitySummaryBase
    {
        public List<EnumValueSummary> Values { get; set; }
    }
}