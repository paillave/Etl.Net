using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class SubFundConfiguration : IEntityTypeConfiguration<SubFund>
    {
        public void Configure(EntityTypeBuilder<SubFund> builder)
        {
            builder.HasBaseType<Security>();
            builder.HasOne(i => i.Sicav).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.SicavId);
            builder.HasOne(i => i.FundAdmin).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.FundAdminId);
            builder.HasOne(i => i.Custodian).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.CustodianId);
            builder.HasOne(i => i.TransferAgent).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.TransferAgentId);
            builder.HasOne(i => i.SubscriptionContact).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.SubscriptionContactId);
            builder.Property(i => i.NavFrequency).HasConversion(new FrequencyTypeValueConverter());
            builder.Property(i => i.InvestmentProcess).HasConversion(new InvestmentProcessTypeValueConverter());
            builder.Property(i => i.Url).HasMaxLength(500);
            builder.Property(i => i.DomicileIso).HasMaxLength(2);
        }
    }
}
