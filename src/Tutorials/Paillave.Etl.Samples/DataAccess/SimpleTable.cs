using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Paillave.Etl.Samples.DataAccess;
public class SimpleTable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<SimpleTableRelated> Relateds { get; set; }
}
public class SimpleTableConfiguration : IEntityTypeConfiguration<SimpleTable>
{
    public void Configure(EntityTypeBuilder<SimpleTable> builder)
    {
        builder.ToTable(nameof(SimpleTable));
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).UseIdentityColumn();
        builder.HasMany(i => i.Relateds).WithOne().OnDelete(DeleteBehavior.Cascade).HasForeignKey(i => i.SimpleTableId);
    }
}


public class SimpleTableRelated
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int SimpleTableId { get; set; }
}
public class SimpleTableRelatedConfiguration : IEntityTypeConfiguration<SimpleTableRelated>
{
    public void Configure(EntityTypeBuilder<SimpleTableRelated> builder)
    {
        builder.ToTable(nameof(SimpleTableRelated));
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).UseIdentityColumn();
    }
}