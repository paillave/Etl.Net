using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class CashConfiguration : IEntityTypeConfiguration<Cash>
    {
        public void Configure(EntityTypeBuilder<Cash> builder)
        {
            builder.HasBaseType<Security>();
        }
    }
}