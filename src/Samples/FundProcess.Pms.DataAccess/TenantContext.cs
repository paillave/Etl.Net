using System;
using System.Collections.Generic;

namespace FundProcess.Pms.DataAccess
{
    public class TenantContext
    {
        public static readonly TenantContext Empty = new TenantContext();
        private TenantContext()
        {
            this.EntityId = 0;
            this.EntityGroupId = 0;
        }
        public TenantContext(int entityId, int entityGroupId)
        {
            if (entityId == 0) throw new ArgumentException("must be greater than 0", nameof(entityId));
            this.EntityId = entityId;
            this.EntityGroupId = entityGroupId;
        }
        public bool IsEmpty => EntityId == 0;
        public int EntityId { get; }
        public int EntityGroupId { get; }
    }
}