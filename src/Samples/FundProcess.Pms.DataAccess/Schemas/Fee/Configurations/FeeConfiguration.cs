using FundProcess.Pms.DataAccess.Schemas.Fee.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Fee.Configurations
{
    public class FeeConfiguration : BelongsToEntityConfigurationBase<Tables.Fee>
    {
        public FeeConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<Tables.Fee> builder)
        {
            builder.ToTable(nameof(Tables.Fee), nameof(Schemas.Fee));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.HasAlternateKey(i => new { i.FeeDefinitionId, i.Date });
            builder.HasOne(i => i.Definition).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.FeeDefinitionId);
        }
    }
}