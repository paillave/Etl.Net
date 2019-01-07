using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class ShareClassConfiguration : IEntityTypeConfiguration<ShareClass>
    {
        public void Configure(EntityTypeBuilder<ShareClass> builder)
        {
            builder.HasBaseType<Security>();
            builder.HasOne(i => i.SubFund).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.SubFundId);
            builder.Property(i => i.DistributionType).HasConversion(new DistributionTypeValueConverter()).HasMaxLength(50);
            builder.Property(i => i.InvestorType).HasConversion(new InvestorTypeValueConverter());
        }
    }
}