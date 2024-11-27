using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Paillave.Etl.Samples.DataAccess
{
    public class SimpleTable
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class SimpleTableConfiguration : IEntityTypeConfiguration<SimpleTable>
    {
        public void Configure(EntityTypeBuilder<SimpleTable> builder)
        {
            builder.ToTable(nameof(SimpleTable));
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).UseIdentityColumn();
        }
    }
}