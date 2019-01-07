using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class DataProviderSecurityConfiguration : IEntityTypeConfiguration<DataProviderSecurity>
    {
        public void Configure(EntityTypeBuilder<DataProviderSecurity> builder)
        {
            builder.ToTable(nameof(DataProviderSecurity), nameof(Schemas.Pms));
            builder.HasKey(i => new { i.SecurityId, i.DataProvider });
            builder.Property(i => i.DataProvider).HasConversion(new DataProviderValueConverter()).IsRequired().HasMaxLength(50);
            builder.Property(i => i.Code).IsRequired().HasMaxLength(50);
        }
    }
}