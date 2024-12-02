using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.Samples.DataAccess
{
    public class TestDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost,1433;Initial Catalog=TestEtl;Persist Security Info=False;User ID=SA;Password=<YourStrong@Passw0rd>;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=true;Connection Timeout=300;");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}