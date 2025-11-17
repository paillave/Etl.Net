using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.EntityFrameworkCore;

public class DbContextWrapper(DbContext dbContext)
{
    public IQueryable<T> Set<T>() where T : class => dbContext.Set<T>().AsNoTracking();
}
