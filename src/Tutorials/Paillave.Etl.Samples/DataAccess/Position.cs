using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Paillave.Etl.Samples.DataAccess;

public class Position
{
    public int CompositionId { get; set; }
    public Composition Composition { get; set; }
    public int SecurityId { get; set; }
    public Security Security { get; set; }
    public decimal Value { get; set; }
}
public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable(nameof(Position));
        builder.HasKey(i => new { i.CompositionId, i.SecurityId });
        builder.HasOne(i => i.Composition).WithMany(i => i.Positions).OnDelete(DeleteBehavior.Cascade).HasForeignKey(i => i.CompositionId);
        builder.HasOne(i => i.Security).WithMany().HasForeignKey(i => i.SecurityId);
    }
}