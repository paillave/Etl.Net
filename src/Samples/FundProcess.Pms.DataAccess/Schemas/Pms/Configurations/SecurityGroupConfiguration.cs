using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class SecurityGroupConfiguration : BelongsToEntityConfigurationBase<SecurityGroup>
    {
        public SecurityGroupConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<SecurityGroup> builder)
        {
            builder.ToTable(nameof(SecurityGroup), nameof(Schemas.Pms));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.Property(i => i.Name).IsRequired().HasMaxLength(250);
        }
    }
}