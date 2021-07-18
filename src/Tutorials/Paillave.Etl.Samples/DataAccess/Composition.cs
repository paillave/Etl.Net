using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Paillave.Etl.Samples.DataAccess
{
    public class Composition
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Portfolio Portfolio { get; set; }
        public int PortfolioId { get; set; }
        public List<Position> Positions { get; set; }
    }
    public class CompositionConfiguration : IEntityTypeConfiguration<Composition>
    {
        public void Configure(EntityTypeBuilder<Composition> builder)
        {
            builder.ToTable(nameof(Composition));
            builder.HasKey(i => i.Id);
            builder.HasAlternateKey(i => new { i.Date, i.PortfolioId });
            builder.Property(i => i.Id).UseIdentityColumn();
            builder.Property(i => i.Date).HasColumnType("DATE");
            builder.HasOne(i => i.Portfolio).WithMany().HasForeignKey(i => i.PortfolioId);
        }
    }
}