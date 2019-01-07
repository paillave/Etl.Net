using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class BondConfiguration : IEntityTypeConfiguration<Bond>
    {
        public void Configure(EntityTypeBuilder<Bond> builder)
        {
            builder.HasBaseType<Security>();
            builder.Property(i => i.PaymentFrequency).HasConversion(new FrequencyTypeValueConverter());
            builder.Property(i => i.CouponType).IsRequired().HasMaxLength(3);
        }
    }
}