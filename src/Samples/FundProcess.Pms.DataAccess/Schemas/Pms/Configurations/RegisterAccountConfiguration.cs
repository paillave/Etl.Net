using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class RegisterAccountConfiguration : BelongsToEntityConfigurationBase<RegisterAccount>
    {
        public RegisterAccountConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<RegisterAccount> builder)
        {
            builder.ToTable(nameof(RegisterAccount), nameof(Schemas.Pms));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.Property(i => i.Name).IsRequired().HasMaxLength(250);
            builder.Property(i => i.SortName).IsRequired().HasMaxLength(250);
            builder.Property(i => i.DealerTaCode).IsRequired().HasMaxLength(250);
            builder.HasOne(i => i.ShareHolder).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.ShareHolderId);
            builder.HasOne(i => i.Distributor).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.DistributorId);
        }
    }
}