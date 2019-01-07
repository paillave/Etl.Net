using FundProcess.Pms.DataAccess.Schemas.UserAccounts.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Globalization;

namespace FundProcess.Pms.DataAccess.Configurations
{
    public class UserLoginConfiguration : BelongsToEntityConfigurationBase<UserLogin>
    {
        public UserLoginConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<UserLogin> builder)
        {
            builder.ToTable(nameof(UserLogin), nameof(Schemas.UserAccounts));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.Property(i => i.IdentityProvider).IsRequired().HasMaxLength(255);
            builder.Property(i => i.Subject).IsRequired().HasMaxLength(255);
            builder.HasOne(i => i.User).WithMany(i => i.Logins).HasForeignKey(i => i.UserId);
            builder.Property(i => i.IsActive).IsRequired().HasDefaultValue(true);
        }
    }
}