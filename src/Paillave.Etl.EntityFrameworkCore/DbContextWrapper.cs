using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.EntityFrameworkCore
{
    public class DbContextWrapper
    {
        private readonly DbContext _dbContext;
        public DbContextWrapper(DbContext dbContext) => _dbContext = dbContext;
        public IQueryable<T> Set<T>() where T : class => _dbContext.Set<T>().AsNoTracking();
    }
}