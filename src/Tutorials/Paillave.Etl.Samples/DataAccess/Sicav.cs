using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Paillave.Etl.Samples.DataAccess
{
    public class Sicav
    {
        public int Id { get; set; }
        public string InternalCode { get; set; }
        public string Name { get; set; }
        public SicavType? Type { get; set; }
    }
    public enum SicavType
    {
        UCITS = 1,
        AIFM = 2
    }
    public class SicavConfiguration : IEntityTypeConfiguration<Sicav>
    {
        public void Configure(EntityTypeBuilder<Sicav> builder)
        {
            builder.ToTable(nameof(Sicav));
            builder.HasKey(i => i.Id);
            builder.HasAlternateKey(i => i.InternalCode);
            builder.Property(i => i.InternalCode).IsRequired();
            builder.Property(i => i.Id).UseIdentityColumn();
            builder.Property(i => i.Type).HasConversion(new EnumToStringConverter<SicavType>());
        }
    }
}