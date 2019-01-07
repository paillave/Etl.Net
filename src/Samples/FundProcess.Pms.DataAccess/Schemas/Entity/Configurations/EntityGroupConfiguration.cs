using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Schemas.Entity.Configurations
{
    public class EntityGroupConfiguration : IEntityTypeConfiguration<EntityGroup>
    {
        public void Configure(EntityTypeBuilder<EntityGroup> builder)
        {
            builder.ToTable(nameof(EntityGroup), nameof(Schemas.Entity));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseSqlServerIdentityColumn();
            builder.Property(i => i.Name).IsRequired().HasMaxLength(50);
        }
    }
}