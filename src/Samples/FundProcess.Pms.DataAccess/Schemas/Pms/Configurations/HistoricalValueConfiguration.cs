using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class HistoricalValueConfiguration : BelongsToEntityConfigurationBase<HistoricalValue>
    {
        public HistoricalValueConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<HistoricalValue> builder)
        {
            builder.ToTable(nameof(HistoricalValue), nameof(Schemas.Pms));
            builder.HasKey(i => new { i.Date, i.SecurityId, i.Type });
            builder.Property(i => i.Type).HasConversion(new HistoricalValueTypeValueConverter()).HasMaxLength(3);
            builder.HasOne(i => i.Security).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.SecurityId);
        }
    }
}