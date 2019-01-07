using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Configurations
{
    public class ManCoConfiguration : IEntityTypeConfiguration<ManCo>
    {
        public void Configure(EntityTypeBuilder<ManCo> builder)
        {
           
        }
    }
}