using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class CfdConfiguration : IEntityTypeConfiguration<Cfd>
    {
        public void Configure(EntityTypeBuilder<Cfd> builder)
        {
            builder.HasBaseType<Derivative>();
        }
    }
}
