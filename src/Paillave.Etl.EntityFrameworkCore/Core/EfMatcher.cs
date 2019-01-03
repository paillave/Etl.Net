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
        private Queue<TEntity> _cachedEntities;
        private int _cacheSize;
        public EfMatcher(Expression<Func<TInLeft, TEntity, bool>> match, int cacheSize = 1000)
        {
            _match = match;
            _cachedEntities = new Queue<TEntity>();
            _cacheSize = cacheSize;
        }

        public TEntity GetMatch(TCtx ctx, TInLeft input)
        {
            var matchExp = _match.ApplyPartialLeft(input);
            var ret = _cachedEntities.AsQueryable().FirstOrDefault(matchExp);
            if (ret != null) return ret;
            var dbSet = ctx.Set<TEntity>();
            ret = dbSet.AsNoTracking().FirstOrDefault(matchExp);
            if (_cachedEntities.Count >= _cacheSize) _cachedEntities.Dequeue();
            _cachedEntities.Enqueue(ret);
            return ret;
        }
    }
}