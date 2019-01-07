using FundProcess.Pms.DataAccess.Schemas.Pms.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Pms.Configurations
{
    public class SecurityGroupItemConfiguration : BelongsToEntityConfigurationBase<SecurityGroupItem>
    {
        public SecurityGroupItemConfiguration(TenantContext tenantContext) : base(tenantContext)
        {
        }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<SecurityGroupItem> builder)
        {
            builder.ToTable(nameof(SecurityGroupItem), nameof(Schemas.Pms));
            builder.HasKey(i => new { i.SecurityId, i.SecurityGroupId });
            builder.HasOne(i => i.SecurityGroup).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.SecurityGroupId);
            builder.HasOne(i => i.Security).WithMany(i => i.Groups).OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.SecurityId);
        }
    }
}