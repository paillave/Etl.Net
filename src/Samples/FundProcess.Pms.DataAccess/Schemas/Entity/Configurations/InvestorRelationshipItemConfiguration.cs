using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Configurations
{
    public class InvestorRelationshipItemConfiguration : BelongsToEntityConfigurationBase<InvestorRelationshipItem>
    {
        public InvestorRelationshipItemConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<InvestorRelationshipItem> builder)
        {
            builder.ToTable(nameof(InvestorRelationshipItem), nameof(Schemas.Entity));
            builder.HasKey(i => new { i.InvestorId, i.InvestorRelationshipId });
            builder.HasOne(i => i.Investor).WithMany(i => i.Relationships).HasForeignKey(i => i.InvestorId);
            builder.HasOne(i=>i.InvestorRelationship).WithMany(i => i.Investors).HasForeignKey(i => i.InvestorRelationshipId);
        }
    }
}