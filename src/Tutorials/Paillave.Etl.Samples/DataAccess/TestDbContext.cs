using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.Samples.DataAccess;

public class TestDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
    }
}
