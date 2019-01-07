using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Configurations
{
    public class InvestorRelationshipConfiguration : BelongsToEntityConfigurationBase<InvestorRelationship>
    {
        public InvestorRelationshipConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<InvestorRelationship> builder)
        {
            builder.ToTable(nameof(InvestorRelationship), nameof(Schemas.Entity));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.Property(i => i.Name).IsRequired().HasMaxLength(250);
        }
    }
}