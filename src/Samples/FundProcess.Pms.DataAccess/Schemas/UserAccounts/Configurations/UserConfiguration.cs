using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Globalization;
using FundProcess.Pms.DataAccess.ValueConverters;
using FundProcess.Pms.DataAccess.Schemas.UserAccounts.Tables;

namespace FundProcess.Pms.DataAccess.Schemas.UserAccounts.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(i => i.Culture).HasDefaultValue(new CultureInfo("en-GB")).HasConversion(new CultureInfoValueConverter()).IsRequired().HasMaxLength(5);
        }
    }
}