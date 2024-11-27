using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Paillave.EntityFrameworkCoreExtension.Core
{
    public class EfMatcher<TInLeft, TEntity, TKey>
    {
        private class CachedEntity(TEntity entity)
        {
            public long Timestamp { get; } = DateTime.Now.ToFileTime();
            public TEntity Entity { get; } = entity;
        }
        private Func<TInLeft, TKey> _getLeftKey;
        private Func<TEntity, TKey> _getRightKey;
        private MatchCriteriaBuilder<TInLeft, TEntity, TKey> _matchCriteriaBuilder;
        private Dictionary<TKey, CachedEntity> _cachedEntities = new Dictionary<TKey, CachedEntity>();
        private int _cacheSize = 1000;
        public DbContext Context { get; }

        private readonly bool _getFullDataset;
        private Func<TInLeft, TEntity> _createIfNotFound = null;
        private IQueryable<TEntity> _query = null;
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
            Context = config.Context;
            _getLeftKey = config.LeftKeyExpression.Compile();
            _getRightKey = config.RightKeyExpression.Compile();
            _matchCriteriaBuilder = MatchCriteriaBuilder.Create(config.LeftKeyExpression, config.RightKeyExpression);
            if (config.GetFullDataset)
            {
                var defaultCache = config.Query.ToList();
                _cacheSize = Math.Max(defaultCache.Count, config.MinCacheSize);
                _cachedEntities = defaultCache
                    .Select(i => new { Key = _getRightKey(i), Value = new CachedEntity(i) })
                    .Where(i => i.Key != null)
                    .GroupBy(i => i.Key)
                    .ToDictionary(i => i.Key, i => i.First().Value, GetEqualityComparer());
            }
            else
            {
                _query = config.Query;
                _cacheSize = config.MinCacheSize;
            }
        }
        public TEntity GetMatch(TInLeft input)
        {
            var inputKey = _getLeftKey(input);
            if (inputKey == null) return default;
            if (_cachedEntities.TryGetValue(inputKey, out var entryFromCache))
                return entryFromCache.Entity;
            if (_getFullDataset) return default;
            var expr = _matchCriteriaBuilder.GetCriteriaExpression(input);

            var ret = _query.FirstOrDefault(expr);

            if (ret == null && _createIfNotFound != null)
            {
                ret = _createIfNotFound(input);
                Context.Add(ret);
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
    {
        public DbContext Context { get; set; }
        public Expression<Func<TInLeft, TKey>> LeftKeyExpression { get; set; }
        public Expression<Func<TEntity, TKey>> RightKeyExpression { get; set; }
        public Func<TInLeft, TEntity> CreateIfNotFound { get; set; }
        /// <summary>
        /// If true, will fill the cache from the start with values that correspond the DefaultDatasetCriteria
        /// </summary>
        /// <value></value>
        public bool GetFullDataset { get; set; }
        public int MinCacheSize { get; set; } = 1000;
        public IQueryable<TEntity> Query { get; set; } = null;
    }
}