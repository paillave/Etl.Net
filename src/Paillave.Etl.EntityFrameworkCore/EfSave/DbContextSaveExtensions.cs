using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Paillave.Etl.EntityFrameworkCore.EfSave
{
    public static class DbContextSaveExtensions
    {
        public static void EfSave<T>(this DbContext context, IList<T> entities, Expression<Func<T, object>> pivotKey = null) where T : class
        {
            EfSaveEngine<T> efSaveEngine = new EfSaveEngine<T>(context, pivotKey);
            efSaveEngine.Save(entities);
        }
    }
}
