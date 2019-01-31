using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class StockConfiguration : IEntityTypeConfiguration<Stock>
    {
        public void Configure(EntityTypeBuilder<Stock> builder)
        {
            builder.HasBaseType<Security>();
        }
    }
}