using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Paillave.Etl.Samples.DataAccess;

public class ShareClass : Security
{
    public string Class { get; set; }
}
public class ShareClassConfiguration : IEntityTypeConfiguration<ShareClass>
{
    public void Configure(EntityTypeBuilder<ShareClass> builder)
    {
        builder.HasBaseType<Security>();
        builder.Property(i => i.Class).IsRequired();
    }
}