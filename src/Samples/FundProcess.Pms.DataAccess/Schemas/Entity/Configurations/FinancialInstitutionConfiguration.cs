using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Configurations
{
    public class FinancialInstitutionConfiguration : IEntityTypeConfiguration<FinancialInstitution>
    {
        public void Configure(EntityTypeBuilder<FinancialInstitution> builder)
        {
            builder.Property(i => i.Type).HasConversion(new FinancialInstitutionTypeValueConverter());
        }
    }
}