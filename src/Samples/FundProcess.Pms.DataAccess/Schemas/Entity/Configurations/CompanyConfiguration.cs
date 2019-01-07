using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FundProcess.Pms.DataAccess.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.HasBaseType<EntityBase>();
            builder.Property(i => i.Name).IsRequired().HasMaxLength(255);
            builder.HasOne(i => i.Contact).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.ContactId);
            builder.Property(i => i.RegistrationNumber).HasMaxLength(50);
            builder.Property(i => i.Url).HasMaxLength(500);
        }
    }
}