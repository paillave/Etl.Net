using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class FxForwardConfiguration : IEntityTypeConfiguration<FxForward>
    {
        public void Configure(EntityTypeBuilder<FxForward> builder)
        {
            builder.HasBaseType<Derivative>();
            builder.Property(i => i.CurrencyToIso).HasMaxLength(3);
        }
    }
}
