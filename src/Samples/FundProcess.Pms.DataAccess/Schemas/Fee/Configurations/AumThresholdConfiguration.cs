using FundProcess.Pms.DataAccess.Schemas.Fee.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Fee.Configurations
{
    public class AumThresholdConfiguration : BelongsToEntityConfigurationBase<AumThreshold>
    {
        public AumThresholdConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<AumThreshold> builder)
        {
            builder.ToTable(nameof(AumThreshold), nameof(Schemas.Fee));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.HasOne(i => i.Fee).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.FeeId);
        }
    }
}