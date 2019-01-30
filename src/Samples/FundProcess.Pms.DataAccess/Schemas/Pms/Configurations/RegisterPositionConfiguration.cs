using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class RegisterPositionConfiguration : BelongsToEntityConfigurationBase<RegisterPosition>
    {
        public RegisterPositionConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<RegisterPosition> builder)
        {
            builder.ToTable(nameof(RegisterPosition), nameof(Schemas.Pms));
            builder.HasIndex(i => new { i.HoldingDate, i.ShareClassId, i.RegisterAccountId, i.BelongsToEntityId });
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.HasOne(i => i.RegisterAccount).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.RegisterAccountId);
            builder.HasOne(i => i.ShareClass).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.ShareClassId);
        }
    }
}