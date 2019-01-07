using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class SecurityTransactionConfiguration : BelongsToEntityConfigurationBase<SecurityTransaction>
    {
        public SecurityTransactionConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<SecurityTransaction> builder)
        {
            builder.ToTable(nameof(SecurityTransaction), nameof(Schemas.Pms));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.Property(i => i.Type).HasConversion(new SecurityTransactionTypeValueConverter()).IsRequired().HasMaxLength(20);
            builder.HasOne(i => i.Portfolio).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.PortfolioId);
            builder.HasOne(i => i.Security).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.SecurityId);
            builder.Property(i => i.StatusCode).IsRequired().HasMaxLength(50);
            builder.Property(i => i.DealDescription).IsRequired();
            builder.Property(i => i.CurrencyCode).IsRequired().HasMaxLength(3);
        }
    }
}