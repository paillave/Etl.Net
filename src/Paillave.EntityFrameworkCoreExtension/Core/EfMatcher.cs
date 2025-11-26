using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Paillave.EntityFrameworkCoreExtension.Core;

public class EfMatcher<TInLeft, TEntity, TKey> where TKey : notnull
{
    private class CachedEntity(TEntity? entity)
    {
        public long Timestamp { get; } = DateTime.Now.ToFileTime();
        public TEntity? Entity { get; } = entity;
    }
    private readonly Func<TInLeft, TKey> _getLeftKey;
    private readonly Func<TEntity, TKey> _getRightKey;
    private int _cacheSize = 1000;
    private readonly MatchCriteriaBuilder<TInLeft, TEntity, TKey> _matchCriteriaBuilder;
    // private readonly Dictionary<TKey, CachedEntity> _cachedEntities = [];
    // private readonly int _cacheSize = 1000;
    // public DbContext Context { get; }

    private static IEqualityComparer<TKey> GetEqualityComparer()
    {
        if (typeof(TKey) == typeof(string))
            return (IEqualityComparer<TKey>)StringComparer.InvariantCultureIgnoreCase;
        else
            return EqualityComparer<TKey>.Default;
    }
    private readonly EfMatcherConfig<TInLeft, TEntity, TKey> _config;
    public EfMatcher(EfMatcherConfig<TInLeft, TEntity, TKey> config)
        => (_config, _getLeftKey, _getRightKey, _matchCriteriaBuilder)
        = (config, config.LeftKeyExpression.Compile(), config.RightKeyExpression.Compile(), MatchCriteriaBuilder.Create(config.LeftKeyExpression, config.RightKeyExpression));

    // if (config.GetFullDataset)
    // {
    //     var defaultCache = config.Query.ToList();
    //     _cacheSize = Math.Max(defaultCache.Count, config.MinCacheSize);
    //     _cachedEntities = defaultCache
    //         .Select(i => new { Key = _getRightKey(i), Value = new CachedEntity(i) })
    //         .Where(i => i.Key != null)
    //         .GroupBy(i => i.Key)
    //         .ToDictionary(i => i.Key, i => i.First().Value, GetEqualityComparer());
    // }
    // else
    // {
    //     _cacheSize = config.MinCacheSize;
    // }
    private Dictionary<TKey, CachedEntity>? _cachedEntities;
    private Dictionary<TKey, CachedEntity> GetCachedEntities(DbContext dbContext)
    {
        if (_cachedEntities == null)
        {
            if (_config.GetFullDataset)
            {
                var defaultCache = _config.GetQuery(dbContext).ToList();
                _cacheSize = Math.Max(defaultCache.Count, _config.MinCacheSize);
                _cachedEntities = defaultCache
                    .Select(i => new { Key = _getRightKey(i), Value = new CachedEntity(i) })
                    .Where(i => i.Key != null)
                    .GroupBy(i => i.Key)
                    .ToDictionary(i => i.Key, i => i.First().Value, GetEqualityComparer());
            }
            else
            {
                _cachedEntities = new(GetEqualityComparer());
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
        if (_config.GetFullDataset) return default;
        var expr = _matchCriteriaBuilder.GetCriteriaExpression(input);
        var query = _config.GetQuery(dbContext);
        var ret = query.FirstOrDefault(expr);

        if (ret == null && _config.CreateIfNotFound != null)
        {
            ret = _config.CreateIfNotFound(input);
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
