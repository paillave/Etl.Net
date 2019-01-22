using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess
{
    public abstract class BelongsToEntityConfigurationBase<T> : IEntityTypeConfiguration<T> where T : class, IBelongsToEntity
    {
        private readonly TenantContext _tenantContext;

        public BelongsToEntityConfigurationBase(TenantContext tenantContext)
        {
            this._tenantContext = tenantContext;
        }
        public void Configure(EntityTypeBuilder<T> builder)
        {
            ConfigureWithoutTenant(builder);
            if (!_tenantContext.IsEmpty)
                builder.HasQueryFilter(i => i.BelongsToEntityId == _tenantContext.EntityId || i.BelongsToEntityId == null);

        }
        protected abstract void ConfigureWithoutTenant(EntityTypeBuilder<T> builder);
    }
}