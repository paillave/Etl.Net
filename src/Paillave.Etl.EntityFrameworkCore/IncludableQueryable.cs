using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Paillave.Etl.EntityFrameworkCore
{
    public class IncludableQueryable<TEntity>
        where TEntity : class
    {
        internal IQueryable<TEntity> Queryable { get; }
        public IncludableQueryable(IQueryable<TEntity> queryable)
        {
            this.Queryable = queryable;
        }
        public SubIncludableQueryable<TEntity, TProp> Include<TProp>(Expression<Func<TEntity, TProp>> navigation) where TProp : class
        {
            return new SubIncludableQueryable<TEntity, TProp>(this.Queryable.Include(navigation));
        }
    }
    public class SubIncludableQueryable<TEntity, TProp> : IncludableQueryable<TEntity>
        where TEntity : class
        where TProp : class
    {
        public SubIncludableQueryable(IIncludableQueryable<TEntity, TProp> queryable) : base(queryable) { }
        public SubIncludableQueryable<TEntity, TSubProp> ThenInclude<TSubProp>(Expression<Func<TProp, TSubProp>> navigation) where TSubProp : class
        {
            return new SubIncludableQueryable<TEntity, TSubProp>((this.Queryable as IIncludableQueryable<TEntity, TProp>).ThenInclude(navigation));
        }
    }
}