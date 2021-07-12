using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.SimpleTutorial.DataAccess
{
    public class SimpleTutorialDbContext: DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost,1433;Database=SimpleTutorial;user=SimpleTutorial;password=TestEtl.TestEtl;MultipleActiveResultSets=True");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}