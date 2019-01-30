using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.EntityFrameworkCore.Core
{
    public class EfMatcher<TInLeft, TEntity, TCtx>
        where TCtx : DbContext
        where TEntity : class
    {
        private Expression<Func<TInLeft, TEntity, bool>> _match;
        private Queue<TEntity> _cachedEntities = new Queue<TEntity>();
        private int _cacheSize = 1000;
        public TCtx Context { get; }
        Func<TInLeft, TEntity> _createIfNotFound = null;
        public EfMatcher(TCtx context, Expression<Func<TInLeft, TEntity, bool>> match, Func<TInLeft, TEntity> createIfNotFound, int cacheSize = 1000)
        {
            _createIfNotFound = createIfNotFound;
            Context = context;
            _match = match;
            _cacheSize = cacheSize;
        }
        public EfMatcher(TCtx context, Expression<Func<TInLeft, TEntity, bool>> match, Func<TInLeft, TEntity> createIfNotFound, Expression<Func<TEntity, bool>> defaultDatasetCriteria, int minCacheSize = 1000)
        {
            _createIfNotFound = createIfNotFound;
            Context = context;
            _match = match;
            var defaultCache = context.Set<TEntity>().Where(defaultDatasetCriteria).ToList();
            _cacheSize = Math.Max(defaultCache.Count, minCacheSize);
            _cachedEntities = new Queue<TEntity>(defaultCache);
        }

        public TEntity GetMatch(TInLeft input)
        {
            var matchExp = _match.ApplyPartialLeft(input);
            var ret = _cachedEntities.AsQueryable().FirstOrDefault(matchExp);
            if (ret != null) return ret;
            var dbSet = Context.Set<TEntity>();
            ret = dbSet.AsNoTracking().FirstOrDefault(matchExp);

            if (ret == null && _createIfNotFound != null)
            {
                ret = _createIfNotFound(input);
                dbSet.Add(ret);
                Context.SaveChanges();
            }

            if (_cachedEntities.Count >= _cacheSize) _cachedEntities.Dequeue();
            if (ret != null) _cachedEntities.Enqueue(ret);
            return ret;
        }
    }
}