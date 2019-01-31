using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class ForexConfiguration : BelongsToEntityConfigurationBase<Forex>
    {
        public ForexConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<Forex> builder)
        {
            builder.ToTable(nameof(Forex), nameof(Schemas.Pms));
            builder.HasKey(i => new { i.Date, i.CurrencyFromIso, i.CurrencyToIso, i.BelongsToEntityId });
            builder.Property(i => i.CurrencyToIso).IsRequired().HasMaxLength(3);
            builder.Property(i => i.CurrencyFromIso).IsRequired().HasMaxLength(3);
        }
    }
}