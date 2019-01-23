using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class SicavConfiguration : BelongsToEntityConfigurationBase<Sicav>
    {
        public SicavConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<Sicav> builder)
        {
            builder.ToTable(nameof(Sicav), nameof(Schemas.Pms));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.Property(i => i.InternalCode).HasMaxLength(50);
            builder.Property(i => i.LegalStructure).HasConversion(new SicavStructureTypeValueConverter());
            builder.HasOne(i => i.ManCo).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.BelongsToEntityId);
            builder.Property(i => i.Name).HasMaxLength(255);
        }
    }
}