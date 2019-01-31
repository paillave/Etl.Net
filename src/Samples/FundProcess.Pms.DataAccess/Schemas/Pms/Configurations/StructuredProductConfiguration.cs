using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class StructuredProductConfiguration : IEntityTypeConfiguration<StructuredProduct>
    {
        public void Configure(EntityTypeBuilder<StructuredProduct> builder)
        {
            builder.HasBaseType<Derivative>();
        }
    }
}
