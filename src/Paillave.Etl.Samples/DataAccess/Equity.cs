using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Paillave.Etl.Samples.DataAccess
{
    public class Equity : Security
    {
        public string Issuer { get; set; }
    }
    public class EquityConfiguration : IEntityTypeConfiguration<Equity>
    {
        public void Configure(EntityTypeBuilder<Equity> builder)
        {
            builder.HasBaseType<Security>();
            builder.Property(i => i.Issuer).IsRequired();
        }
    }
}