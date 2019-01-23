using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class IndexConfiguration : IEntityTypeConfiguration<Index>
    {
        public void Configure(EntityTypeBuilder<Index> builder)
        {
            builder.HasBaseType<Derivative>();
        }
    }
}
