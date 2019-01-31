using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class CommodityConfiguration : IEntityTypeConfiguration<Commodity>
    {
        public void Configure(EntityTypeBuilder<Commodity> builder)
        {
            builder.HasBaseType<Derivative>();
        }
    }
}
