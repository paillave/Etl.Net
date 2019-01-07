using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess
{
    public abstract class BelongsToEntityGroupConfigurationBase<T> : IEntityTypeConfiguration<T> where T : class, IBelongsToEntityGroup
    {
        private readonly TenantContext _tenantContext;

        public BelongsToEntityGroupConfigurationBase(TenantContext tenantContext)
        {
            this._tenantContext = tenantContext;
        }
        public void Configure(EntityTypeBuilder<T> builder)
        {
            ConfigureWithoutTenant(builder);
                builder.HasQueryFilter(i => i.BelongsToEntityGroupId == _tenantContext.EntityGroupId || i.BelongsToEntityGroupId == null);
        }
        protected abstract void ConfigureWithoutTenant(EntityTypeBuilder<T> builder);
    }
}