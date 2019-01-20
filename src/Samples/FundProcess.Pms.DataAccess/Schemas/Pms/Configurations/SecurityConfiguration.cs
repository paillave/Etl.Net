using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class SecurityConfiguration : BelongsToEntityConfigurationBase<Security>
    {
        public SecurityConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<Security> builder)
        {
            builder.ToTable(nameof(Security), nameof(Schemas.Pms));
            builder.HasAlternateKey(i => new { i.BelongsToEntityId, i.InternalCode });
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            // builder.Property(i => i.Type).HasConversion(new SecurityTypeValueConverter());
            builder.HasOne(i => i.Benchmark).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.BenchmarkId);
            builder.Property(i => i.PricingFrequency).HasConversion(new FrequencyTypeValueConverter());
            builder.Property(i => i.Name).IsRequired().HasMaxLength(250);
            builder.Property(i => i.InternalCode).IsRequired().HasMaxLength(50);
            builder.Property(i => i.Isin).HasMaxLength(12);
            builder.Property(i => i.CurrencyIso).HasMaxLength(3);
            builder.Property(i => i.CountryCode).HasMaxLength(2);
            builder.Property(i => i.ClassificationStrategy);
        }
    }
}