using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class FutureConfiguration : IEntityTypeConfiguration<Future>
    {
        public void Configure(EntityTypeBuilder<Future> builder)
        {
            builder.HasBaseType<Derivative>();
        }
    }
}
