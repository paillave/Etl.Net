using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.EntityFrameworkCore.Core
{
    public class EfMatcher<TInLeft, TEntity, TCtx, TKey>
        where TCtx : DbContext
        where TEntity : class
    {
<<<<<<< HEAD
        private class CachedEntity
        {
            public CachedEntity(TEntity entity)
            {
                this.Timestamp = DateTime.Now.ToFileTime();
                this.Entity = entity;

            }
            public long Timestamp { get; }
            public TEntity Entity { get; }
        }
        private Func<TInLeft, TKey> _getLeftKey;
        private Func<TEntity, TKey> _getRightKey;
        private MatchCriteriaBuilder<TInLeft, TEntity, TKey> _matchCriteriaBuilder;
        private Dictionary<TKey, CachedEntity> _cachedEntities = new Dictionary<TKey, CachedEntity>();
=======
        private Expression<Func<TInLeft, TEntity, bool>> _match;
        private Queue<TEntity> _cachedEntities = new Queue<TEntity>();
>>>>>>> 39086887d648599f9b12d6108fc67e0480fcf20c
        private int _cacheSize = 1000;
        public TCtx Context { get; }
        Func<TInLeft, TEntity> _createIfNotFound = null;
        public EfMatcher(TCtx context, Expression<Func<TInLeft, TKey>> leftKeyExpression, Expression<Func<TEntity, TKey>> rightKeyExpression, Func<TInLeft, TEntity> createIfNotFound, int cacheSize = 1000)
        {
            _createIfNotFound = createIfNotFound;
            Context = context;
            // _leftKeyExpression = leftKeyExpression;
            _getLeftKey = leftKeyExpression.Compile();
            // _rightKeyExpression = rightKeyExpression;
            _getRightKey = rightKeyExpression.Compile();
            _matchCriteriaBuilder = MatchCriteriaBuilder.Create(leftKeyExpression, rightKeyExpression);
            _cacheSize = cacheSize;
        }
        public EfMatcher(TCtx context, Expression<Func<TInLeft, TKey>> leftKeyExpression, Expression<Func<TEntity, TKey>> rightKeyExpression, Func<TInLeft, TEntity> createIfNotFound, Expression<Func<TEntity, bool>> defaultDatasetCriteria, int minCacheSize = 1000)
        {
            _createIfNotFound = createIfNotFound;
            Context = context;
            // _leftKeyExpression = leftKeyExpression;
            _getLeftKey = leftKeyExpression.Compile();
            // _rightKeyExpression = rightKeyExpression;
            _getRightKey = rightKeyExpression.Compile();
            _matchCriteriaBuilder = MatchCriteriaBuilder.Create(leftKeyExpression, rightKeyExpression);
            var defaultCache = context.Set<TEntity>().Where(defaultDatasetCriteria).ToList();
            _cacheSize = Math.Max(defaultCache.Count, minCacheSize);
<<<<<<< HEAD
            _cachedEntities = defaultCache.ToDictionary(_getRightKey, i => new CachedEntity(i));
=======
            _cachedEntities = new Queue<TEntity>(defaultCache);
>>>>>>> 39086887d648599f9b12d6108fc67e0480fcf20c
        }

        public TEntity GetMatch(TInLeft input)
        {
            var inputKey = _getLeftKey(input);
            if (_cachedEntities.TryGetValue(inputKey, out var entryFromCache))
                return entryFromCache.Entity;
            var dbSet = Context.Set<TEntity>();
            var expr = _matchCriteriaBuilder.GetCriteriaExpression(input);
            var ret = dbSet.AsNoTracking().FirstOrDefault(expr);

            if (ret == null && _createIfNotFound != null)
            {
                ret = _createIfNotFound(input);
                dbSet.Add(ret);
                Context.SaveChanges();
            }

<<<<<<< HEAD
            if (_cachedEntities.Count >= _cacheSize)
            {
                var toRemove = _cachedEntities.OrderBy(i => i.Value.Timestamp).FirstOrDefault();
                _cachedEntities.Remove(toRemove.Key);
            }
            _cachedEntities[inputKey] = new CachedEntity(ret);
=======
            if (_cachedEntities.Count >= _cacheSize) _cachedEntities.Dequeue();
            if (ret != null) _cachedEntities.Enqueue(ret);
>>>>>>> 39086887d648599f9b12d6108fc67e0480fcf20c
            return ret;
        }
    }
}