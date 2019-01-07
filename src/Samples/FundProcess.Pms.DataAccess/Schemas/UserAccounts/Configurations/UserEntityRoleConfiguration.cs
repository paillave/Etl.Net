using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using FundProcess.Pms.DataAccess.ValueConverters;
using FundProcess.Pms.DataAccess.Schemas.UserAccounts.Tables;

namespace FundProcess.Pms.DataAccess.Configurations
{
    public class UserEntityRoleConfiguration : BelongsToEntityConfigurationBase<UserEntityRole>
    {
        public UserEntityRoleConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<UserEntityRole> builder)
        {
            builder.ToTable(nameof(UserEntityRole), nameof(Schemas.UserAccounts));
            builder.HasKey(i => new { i.UserId, i.CompanyId, i.ApplicationRole });
            builder.HasOne(i => i.Company).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.CompanyId);
            builder.Property(i => i.ApplicationRole).HasConversion(new ApplicationRoleValueConverter()).IsRequired().HasMaxLength(50);
            builder.HasOne(i => i.User).WithMany(i => i.Roles).HasForeignKey(i => i.UserId);
        }
    }
}