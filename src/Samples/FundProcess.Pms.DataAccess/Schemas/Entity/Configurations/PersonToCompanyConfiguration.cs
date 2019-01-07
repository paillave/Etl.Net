using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Configurations
{
    public class PersonToCompanyConfiguration : BelongsToEntityConfigurationBase<PersonToCompany>
    {
        public PersonToCompanyConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<PersonToCompany> builder)
        {
            builder.ToTable(nameof(PersonToCompany), nameof(Schemas.Entity));
            builder.HasKey(i => new { i.CompanyId, i.PersonId });
            builder.HasOne(i => i.Person).WithMany(i => i.Companies).OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.PersonId);
            builder.HasOne(i => i.Company).WithMany(i => i.People).OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.CompanyId);
        }
    }
}