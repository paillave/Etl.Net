using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Paillave.Etl.Samples.DataAccess;

public abstract class Security
{
    public int Id { get; set; }
    public required string InternalCode { get; set; }
    public string? Isin { get; set; }
    public required string Name { get; set; }
}
public class SecurityConfiguration : IEntityTypeConfiguration<Security>
{
    public void Configure(EntityTypeBuilder<Security> builder)
    {
        builder.ToTable(nameof(Security));
        builder.HasKey(i => i.Id);
        builder.HasAlternateKey(i => i.InternalCode);
        builder.Property(i => i.Id).UseIdentityColumn();
    }
}