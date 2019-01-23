using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class SwapConfiguration : IEntityTypeConfiguration<Swap>
    {
        public void Configure(EntityTypeBuilder<Swap> builder)
        {
            builder.HasBaseType<Derivative>();
        }
    }
}
