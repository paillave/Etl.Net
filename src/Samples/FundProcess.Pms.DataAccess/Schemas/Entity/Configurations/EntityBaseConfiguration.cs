using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;

namespace FundProcess.Pms.DataAccess.Configurations
{
    public class EntityBaseConfiguration : BelongsToEntityConfigurationBase<EntityBase>
    {
        public EntityBaseConfiguration(TenantContext tenantContext) : base(tenantContext) { }

        protected override void ConfigureWithoutTenant(EntityTypeBuilder<EntityBase> builder)
        {
            builder.ToTable(nameof(EntityBase), nameof(Schemas.Entity));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.Property(i => i.Email).HasMaxLength(250);
            builder.Property(i => i.Address);
            builder.Property(i => i.CountryCode).HasMaxLength(2);
            builder.Property(i => i.PhoneNumber).HasMaxLength(50);
            builder.Property(i => i.ConnectionString).HasMaxLength(512);
            builder.Property(i => i.IsActive).IsRequired().HasDefaultValue(true);
            builder.HasOne(i => i.Group).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.EntityGroupId);
        }
    }
}