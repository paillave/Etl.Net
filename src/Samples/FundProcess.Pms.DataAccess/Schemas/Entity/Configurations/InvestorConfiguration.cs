using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using FundProcess.Pms.DataAccess.ValueConverters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Configurations
{
    public class InvestorConfiguration : BelongsToEntityConfigurationBase<Investor>
    {
        public InvestorConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<Investor> builder)
        {
            builder.ToTable(nameof(Investor), nameof(Schemas.Entity));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.HasOne(i => i.Entity).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.EntityId);
            builder.Property(i => i.Type).HasConversion(new InvestorTypeValueConverter());
            builder.HasOne(i => i.Intermediary).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.IntermediaryId);
            builder.HasOne(i => i.InternalResponsible).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.InternalResponsibleId);
        }
    }
}