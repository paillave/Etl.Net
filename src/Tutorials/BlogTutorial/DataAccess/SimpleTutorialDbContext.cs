using Microsoft.EntityFrameworkCore;

namespace BlogTutorial.DataAccess
{
    public class SimpleTutorialDbContext : DbContext
    {
        private readonly string _connectionString = null;
        public SimpleTutorialDbContext() { }
        public SimpleTutorialDbContext(string connectionString) => _connectionString = connectionString;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString ?? @"Server=localhost,1433;Database=BlogTutorial;user=BlogTutorial;password=TestEtl.TestEtl;MultipleActiveResultSets=True");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}