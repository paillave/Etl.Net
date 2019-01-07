using FundProcess.Pms.DataAccess.Schemas.Fee.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Fee.Configurations
{
    public class FeeDefinitionConfiguration : BelongsToEntityConfigurationBase<FeeDefinition>
    {
        public FeeDefinitionConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<FeeDefinition> builder)
        {
            builder.ToTable(nameof(FeeDefinition), nameof(Schemas.Fee));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.HasOne(i => i.Portfolio).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.PortfolioId);
            builder.HasOne(i => i.ShareClass).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.ShareClassId);
            builder.HasOne(i => i.Sicav).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.SicavId);
            builder.HasOne(i => i.RegisterAccount).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.RegisterAccountId);
            builder.HasOne(i => i.ThirdParty).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.ThirdPartyId);
            builder.Property(i => i.AssetPart).HasConversion(new AssetPartValueConverter());
            builder.HasOne(i => i.ManCoSecurities).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.ManCoSecuritiesId);
            builder.Property(i => i.FeeType).HasConversion(new FeeTypeValueConverter());
            builder.Property(i => i.PaymentFrequency).HasConversion(new FrequencyTypeValueConverter());
        }
    }
}