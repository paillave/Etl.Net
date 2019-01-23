using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class PortfolioCompositionConfiguration : BelongsToEntityConfigurationBase<PortfolioComposition>
    {
        public PortfolioCompositionConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<PortfolioComposition> builder)
        {
            builder.ToTable(nameof(PortfolioComposition), nameof(Schemas.Pms));
            builder.HasAlternateKey(i => new { i.Date, i.PortfolioId, i.BelongsToEntityId });
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.HasOne(i => i.Portfolio).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.PortfolioId);
        }
    }
}