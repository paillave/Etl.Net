using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Paillave.EntityFrameworkCoreExtension.Core;

public class EfMatcher<TInLeft, TEntity, TKey>(EfMatcherConfig<TInLeft, TEntity, TKey> config)
     where TKey : notnull
{
    private class CachedEntity(TEntity? entity)
    {
        public long Timestamp { get; } = DateTime.Now.ToFileTime();
        public TEntity? Entity { get; } = entity;
    }
    private readonly Func<TInLeft, TKey> _getLeftKey = config.LeftKeyExpression.Compile();
    private readonly Func<TEntity, TKey> _getRightKey = config.RightKeyExpression.Compile();
    private int _cacheSize = 1000;
    private readonly MatchCriteriaBuilder<TInLeft, TEntity, TKey> _matchCriteriaBuilder = MatchCriteriaBuilder.Create(config.LeftKeyExpression, config.RightKeyExpression);

    private static IEqualityComparer<TKey> GetEqualityComparer()
    {
        if (typeof(TKey) == typeof(string))
            return (IEqualityComparer<TKey>)StringComparer.InvariantCultureIgnoreCase;
        else
            return EqualityComparer<TKey>.Default;
    }
    private Dictionary<TKey, CachedEntity>? _cachedEntities;
    private Dictionary<TKey, CachedEntity> GetCachedEntities(DbContext dbContext)
    {
        if (_cachedEntities == null)
        {
            if (config.GetFullDataset)
            {
                var defaultCache = config.GetQuery(dbContext).ToList();
                _cacheSize = Math.Max(defaultCache.Count, config.MinCacheSize);
                _cachedEntities = defaultCache
                    .Select(i => new { Key = _getRightKey(i), Value = new CachedEntity(i) })
                    .Where(i => i.Key != null)
                    .GroupBy(i => i.Key)
                    .ToDictionary(i => i.Key, i => i.First().Value, GetEqualityComparer());
            }
            else
            {
                _cachedEntities = new(GetEqualityComparer());
                _cacheSize = config.MinCacheSize;
            }
        }
        return _cachedEntities;
    }
    public TEntity? GetMatch(TInLeft input, DbContext dbContext)
    {
        var inputKey = _getLeftKey(input);
        if (inputKey == null) return default;
        var cachedEntities = GetCachedEntities(dbContext);
        if (cachedEntities.TryGetValue(inputKey, out var entryFromCache))
            return entryFromCache.Entity;
        if (config.GetFullDataset) return default;
        var expr = _matchCriteriaBuilder.GetCriteriaExpression(input);
        var query = config.GetQuery(dbContext);
        var ret = query.FirstOrDefault(expr);

        if (ret == null && config.CreateIfNotFound != null)
        {
            ret = config.CreateIfNotFound(input);
            if (ret == null) throw new InvalidOperationException("CreateIfNotFound returned null");
            dbContext.Add(ret);
            dbContext.SaveChanges();
        }

        if (cachedEntities.Count >= _cacheSize)
        {
            var toRemove = cachedEntities.OrderBy(i => i.Value.Timestamp).FirstOrDefault();
            cachedEntities.Remove(toRemove.Key);
        }
        cachedEntities[inputKey] = new CachedEntity(ret);
        return ret;
    }
}
public class EfMatcherConfig<TInLeft, TEntity, TKey>
{
    public required Expression<Func<TInLeft, TKey>> LeftKeyExpression { get; set; }
    public required Expression<Func<TEntity, TKey>> RightKeyExpression { get; set; }
    public required Func<TInLeft, TEntity>? CreateIfNotFound { get; set; }
    public bool GetFullDataset { get; set; }
    public int MinCacheSize { get; set; } = 1000;
    public required Func<DbContext, IQueryable<TEntity>> GetQuery { get; set; }
}
