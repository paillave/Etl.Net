using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Paillave.Etl.Samples.DataAccess
{
    public class Portfolio
    {
        public int Id { get; set; }
        public string InternalCode { get; set; }
        public string Name { get; set; }
        public Sicav Sicav { get; set; }
        public int SicavId { get; set; }
    }
    public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
    {
        public void Configure(EntityTypeBuilder<Portfolio> builder)
        {
            builder.ToTable(nameof(Portfolio));
            builder.HasKey(i => i.Id);
            builder.HasAlternateKey(i => i.InternalCode);
            builder.Property(i => i.Id).UseIdentityColumn();
            builder.Property(i => i.InternalCode).IsRequired();
            builder.HasOne(i => i.Sicav).WithMany().HasForeignKey(i => i.SicavId);
        }
    }
}