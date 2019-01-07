using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class EquityConfiguration : IEntityTypeConfiguration<Equity>
    {
        public void Configure(EntityTypeBuilder<Equity> builder)
        {
            builder.HasBaseType<Security>();
        }
    }
}
