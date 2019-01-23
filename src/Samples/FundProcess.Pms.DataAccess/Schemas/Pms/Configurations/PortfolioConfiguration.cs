using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
    {
        public void Configure(EntityTypeBuilder<Portfolio> builder)
        {
            builder.HasBaseType<Security>();
            builder.HasOne(i => i.FundAdmin).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.FundAdminId);
            builder.HasOne(i => i.Custodian).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.CustodianId);
            builder.Property(i => i.NavFrequency).HasConversion(new FrequencyTypeValueConverter());
        }
    }
}
