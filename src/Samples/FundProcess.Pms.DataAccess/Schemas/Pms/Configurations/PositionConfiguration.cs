using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class PositionConfiguration : BelongsToEntityConfigurationBase<Position>
    {
        public PositionConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<Position> builder)
        {
            builder.ToTable(nameof(Position), nameof(Schemas.Pms));
            builder.HasIndex(i => new { i.PortfolioCompositionId, i.SecurityId, i.BelongsToEntityId });
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.HasOne(i => i.PortfolioComposition).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.PortfolioCompositionId);
            builder.HasOne(i => i.Security).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.SecurityId);
        }
    }
}