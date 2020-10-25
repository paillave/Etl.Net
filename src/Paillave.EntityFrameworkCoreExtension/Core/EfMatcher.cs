using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Paillave.EntityFrameworkCoreExtension.Core
{
    public class EfMatcher<TInLeft, TEntity, TKey>
        where TEntity : class
    {
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
        private int _cacheSize = 1000;
        public DbContext Context { get; }

        private readonly bool _getFullDataset;
        private Func<TInLeft, TEntity> _createIfNotFound = null;
        private Func<IQueryable<TEntity>, IQueryable<TEntity>> _includeInstruction = null;
        private static IEqualityComparer<TKey> GetEqualityComparer()
        {
            if (typeof(TKey) == typeof(string))
                return StringComparer.InvariantCultureIgnoreCase as IEqualityComparer<TKey>;
            else
                return EqualityComparer<TKey>.Default;
        }
        public EfMatcher(EfMatcherConfig<TInLeft, TEntity, TKey> config)
        {
            _getFullDataset = config.GetFullDataset;
            _createIfNotFound = config.CreateIfNotFound;
            _includeInstruction = config.IncludeClause;
            Context = config.Context;
            _getLeftKey = config.LeftKeyExpression.Compile();
            _getRightKey = config.RightKeyExpression.Compile();
            _matchCriteriaBuilder = MatchCriteriaBuilder.Create(config.LeftKeyExpression, config.RightKeyExpression, config.WhereClause);
            if (config.GetFullDataset)
            {
                var query = (_includeInstruction == null ? config.Context.Set<TEntity>() : _includeInstruction(config.Context.Set<TEntity>()));
                if (config.WhereClause != null)
                    query = query.Where(config.WhereClause);

                var defaultCache = query.ToList();
                _cacheSize = Math.Max(defaultCache.Count, config.MinCacheSize);
                _cachedEntities = defaultCache
                    .Select(i => new { Key = _getRightKey(i), Value = new CachedEntity(i) })
                    .Where(i => i.Key != null)
                    .GroupBy(i=>i.Key)
                    .ToDictionary(i => i.Key, i => i.First().Value, GetEqualityComparer());
            }
            else
            {
                _cacheSize = config.MinCacheSize;
            }
        }
        public TEntity GetMatch(TInLeft input)
        {
            var inputKey = _getLeftKey(input);
            if (inputKey == null) return null;
            if (_cachedEntities.TryGetValue(inputKey, out var entryFromCache))
                return entryFromCache.Entity;
            if (_getFullDataset) return default;
            var dbSet = Context.Set<TEntity>();
            var expr = _matchCriteriaBuilder.GetCriteriaExpression(input);

            var queryable = _includeInstruction == null ? dbSet : _includeInstruction(dbSet);
            var ret = queryable.AsNoTracking().FirstOrDefault(expr);

            if (ret == null && _createIfNotFound != null)
            {
                ret = _createIfNotFound(input);
                dbSet.Add(ret);
                Context.SaveChanges();
            }

            if (_cachedEntities.Count >= _cacheSize)
            {
                var toRemove = _cachedEntities.OrderBy(i => i.Value.Timestamp).FirstOrDefault();
                _cachedEntities.Remove(toRemove.Key);
            }
            _cachedEntities[inputKey] = new CachedEntity(ret);
            return ret;
        }
    }
    public class EfMatcherConfig<TInLeft, TEntity, TKey>
        where TEntity : class
    {
        public DbContext Context { get; set; }
        public Expression<Func<TInLeft, TKey>> LeftKeyExpression { get; set; }
        public Expression<Func<TEntity, TKey>> RightKeyExpression { get; set; }
        public Func<TInLeft, TEntity> CreateIfNotFound { get; set; }
        /// <summary>
        /// Criteria that is applied on top of the selection criteria on the EntitySet
        /// </summary>
        /// <value></value>
        public Expression<Func<TEntity, bool>> WhereClause { get; set; }
        /// <summary>
        /// If true, will fill the cache from the start with values that correspond the DefaultDatasetCriteria
        /// </summary>
        /// <value></value>
        public bool GetFullDataset { get; set; }
        public int MinCacheSize { get; set; } = 1000;
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> IncludeClause { get; set; } = null;
    }
}