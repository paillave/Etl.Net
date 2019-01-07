using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class DerivativeConfiguration : IEntityTypeConfiguration<Derivative>
    {
        public void Configure(EntityTypeBuilder<Derivative> builder)
        {
            builder.HasBaseType<Security>();
            builder.HasOne(i => i.Counterparty).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.CounterpartyId);
        }
    }
}
