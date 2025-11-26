using System;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;
using Microsoft.EntityFrameworkCore;
using Paillave.EntityFrameworkCoreExtension.Core;
using Paillave.Etl.Reactive.Core;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Paillave.Etl.EntityFrameworkCore
{
    #region Non correlated args builder
    public class EfCoreLookupArgsBuilder<TIn>
    {
        internal IStream<TIn> SourceStream { get; }
        internal EfCoreLookupArgsBuilder(IStream<TIn> sourceStream) => (SourceStream) = (sourceStream);
        public EfCoreLookupArgsBuilder<TIn, TEntity> Set<TEntity>(string keyedConnection = null) where TEntity : class => new EfCoreLookupArgsBuilder<TIn, TEntity>(this, o => o.Set<TEntity>(), keyedConnection);
        public EfCoreLookupArgsBuilder<TIn, TEntity> Query<TEntity>(Func<DbContextWrapper, IQueryable<TEntity>> getQuery, string keyedConnection = null) where TEntity : class => new EfCoreLookupArgsBuilder<TIn, TEntity>(this, getQuery, keyedConnection);
    }
    public class EfCoreLookupArgsBuilder<TIn, TEntity>
    {
        internal EfCoreLookupArgsBuilder<TIn> Parent { get; }
        internal Func<DbContextWrapper, IQueryable<TEntity>> GetQuery { get; set; }
        internal string KeyedConnection { get; private set; }
        internal EfCoreLookupArgsBuilder(EfCoreLookupArgsBuilder<TIn> parent, Func<DbContextWrapper, IQueryable<TEntity>> getQuery, string keyedConnection = null) => (Parent, KeyedConnection, GetQuery) = (parent, keyedConnection, getQuery);
        public EfCoreLookupArgsBuilder<TIn, TEntity, TKey> On<TKey>(Expression<Func<TIn, TKey>> getLeftStreamKey, Expression<Func<TEntity, TKey>> getEntityStreamKey)
            => new EfCoreLookupArgsBuilder<TIn, TEntity, TKey>(this, getLeftStreamKey, getEntityStreamKey);
    }
    public class EfCoreLookupArgsBuilder<TIn, TEntity, TKey>
    {
        internal EfCoreLookupArgsBuilder<TIn, TEntity> Parent { get; }
        internal Expression<Func<TIn, TKey>> GetLeftStreamKey { get; }
        internal Expression<Func<TEntity, TKey>> GetEntityStreamKey { get; }
        internal EfCoreLookupArgsBuilder(EfCoreLookupArgsBuilder<TIn, TEntity> parent, Expression<Func<TIn, TKey>> getLeftStreamKey, Expression<Func<TEntity, TKey>> getEntityStreamKey)
            => (Parent, GetLeftStreamKey, GetEntityStreamKey) = (parent, getLeftStreamKey, getEntityStreamKey);
        public EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey> Select<TOut>(Func<TIn, TEntity, TOut> resultSelector)
            => new EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey>(this, resultSelector);
    }
    public class EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey>
    {
        private readonly EfCoreLookupArgsBuilder<TIn, TEntity, TKey> _parent;
        private readonly Func<TIn, TEntity, TOut> _resultSelector;
        private Func<TIn, TEntity> _createIfNotFound = null;
        private int _cacheSize = 1000;
        private bool _fullDataset = true;
        // private EfCoreLookupArgs<TIn, TIn, TEntity, TOut, TKey, TOut> Args { get; set; }
        internal EfCoreLookupArgs<TIn, TIn, TEntity, TOut, TKey, TOut> BuildArgs()
            => new EfCoreLookupArgs<TIn, TIn, TEntity, TOut, TKey, TOut>
            {
                SourceStream = _parent.Parent.Parent.SourceStream,
                GetLeftStreamKey = _parent.GetLeftStreamKey,
                GetEntityStreamKey = _parent.GetEntityStreamKey,
                ResultSelector = _resultSelector,
                GetInputValue = i => i,
                GetOutputValue = (i, j) => j,
                Query = _parent.Parent.GetQuery,
                KeyedConnection = _parent.Parent.KeyedConnection,
                GetFullDataset = _fullDataset,
                CacheSize = _cacheSize,
                CreateIfNotFound = _createIfNotFound
            };
        internal EfCoreLookupArgsBuilder(EfCoreLookupArgsBuilder<TIn, TEntity, TKey> parent, Func<TIn, TEntity, TOut> resultSelector)
            => (_parent, _resultSelector) = (parent, resultSelector);
        public EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey> CreateIfNotFound(Func<TIn, TEntity> createIfNotFound)
        {
            this._createIfNotFound = createIfNotFound;
            return this;
        }
        public EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey> CacheSize(int cacheSize)
        {
            this._cacheSize = cacheSize;
            return this;
        }
        public EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey> CacheFullDataset(bool fullDataset = true)
        {
            this._fullDataset = fullDataset;
            return this;
        }
        public EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey> NoCacheFullDataset(bool notFullDataset = true)
        {
            this._fullDataset = !notFullDataset;
            return this;
        }
    }
    #endregion

    #region Correlated args builder
    public class EfCoreLookupCorrelatedArgsBuilder<TIn>
    {
        internal IStream<Correlated<TIn>> SourceStream { get; }
        // internal EfCoreLookupCorrelatedArgsBuilder(IStream<Correlated<TIn>> sourceStream) => (SourceStream) = (sourceStream);
        // public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> Set<TEntity>(string keyedConnection = null) where TEntity : class => new EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity>(this, keyedConnection);






        internal EfCoreLookupCorrelatedArgsBuilder(IStream<Correlated<TIn>> sourceStream) => (SourceStream) = (sourceStream);
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> Set<TEntity>(string keyedConnection = null) where TEntity : class => new EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity>(this, o => o.Set<TEntity>(), keyedConnection);
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> Query<TEntity>(Func<DbContextWrapper, IQueryable<TEntity>> getQuery, string keyedConnection = null) where TEntity : class => new EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity>(this, getQuery, keyedConnection);
    }
    public class EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> where TEntity : class
    {
        internal EfCoreLookupCorrelatedArgsBuilder<TIn> Parent { get; }
        internal Func<DbContextWrapper, IQueryable<TEntity>> GetQuery { get; set; }
        internal string KeyedConnection { get; private set; } = null;
        internal EfCoreLookupCorrelatedArgsBuilder(EfCoreLookupCorrelatedArgsBuilder<TIn> parent, Func<DbContextWrapper, IQueryable<TEntity>> getQuery, string keyedConnection) => (Parent, KeyedConnection, GetQuery) = (parent, keyedConnection, getQuery);
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TKey> On<TKey>(Expression<Func<TIn, TKey>> getLeftStreamKey, Expression<Func<TEntity, TKey>> getEntityStreamKey)
            => new EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TKey>(this, getLeftStreamKey, getEntityStreamKey);
    }
    public class EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TKey> where TEntity : class
    {
        internal EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> Parent { get; }
        internal Expression<Func<TIn, TKey>> GetLeftStreamKey { get; }
        internal Expression<Func<TEntity, TKey>> GetEntityStreamKey { get; }
        internal EfCoreLookupCorrelatedArgsBuilder(EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> parent, Expression<Func<TIn, TKey>> getLeftStreamKey, Expression<Func<TEntity, TKey>> getEntityStreamKey)
            => (Parent, GetLeftStreamKey, GetEntityStreamKey) = (parent, getLeftStreamKey, getEntityStreamKey);
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TOut, TKey> Select<TOut>(Func<TIn, TEntity, TOut> resultSelector)
            => new EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TOut, TKey>(this, resultSelector);
    }

    public class EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TOut, TKey> where TEntity : class
    {
        private readonly EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TKey> _parent;
        private readonly Func<TIn, TEntity, TOut> _resultSelector;
        private Func<TIn, TEntity> _createIfNotFound = null;
        private int _cacheSize = 1000;
        private bool _fullDataset = true;
        // private EfCoreLookupArgs<TIn, TIn, TEntity, TOut, TKey, TOut> Args { get; set; }
        internal EfCoreLookupArgs<Correlated<TIn>, TIn, TEntity, TOut, TKey, Correlated<TOut>> BuildArgs()
            => new EfCoreLookupArgs<Correlated<TIn>, TIn, TEntity, TOut, TKey, Correlated<TOut>>
            {
                SourceStream = _parent.Parent.Parent.SourceStream,
                GetLeftStreamKey = _parent.GetLeftStreamKey,
                GetEntityStreamKey = _parent.GetEntityStreamKey,
                ResultSelector = _resultSelector,
                GetInputValue = i => i.Row,
                GetOutputValue = (i, j) => new Correlated<TOut> { Row = j, CorrelationKeys = i.CorrelationKeys },
                Query = _parent.Parent.GetQuery,
                KeyedConnection = _parent.Parent.KeyedConnection,
                CacheSize = _cacheSize,
                CreateIfNotFound = _createIfNotFound,
                GetFullDataset = _fullDataset
            };
        internal EfCoreLookupCorrelatedArgsBuilder(EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TKey> parent, Func<TIn, TEntity, TOut> resultSelector)
            => (_parent, _resultSelector) = (parent, resultSelector);
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TOut, TKey> CreateIfNotFound(Func<TIn, TEntity> createIfNotFound)
        {
            this._createIfNotFound = createIfNotFound;
            return this;
        }
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TOut, TKey> CacheSize(int cacheSize)
        {
            this._cacheSize = cacheSize;
            return this;
        }
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TOut, TKey> CacheFullDataset(bool fullDataset = true)
        {
            this._fullDataset = fullDataset;
            return this;
        }
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity, TOut, TKey> NoCacheFullDataset(bool notFullDataset = true)
        {
            this._fullDataset = !notFullDataset;
            return this;
        }
    }
    #endregion
    public class EfCoreLookupArgs<TIn, TValueIn, TEntity, TValueOut, TKey, TOut>
    {
        public Func<TIn, TValueIn> GetInputValue { get; set; }
        // public Func<TValueTIn, bool> Where { get; set; }
        public Func<TIn, TValueOut, TOut> GetOutputValue { get; set; }
        public IStream<TIn> SourceStream { get; set; }
        public Expression<Func<TValueIn, TKey>> GetLeftStreamKey { get; set; }
        public Expression<Func<TEntity, TKey>> GetEntityStreamKey { get; set; }
        public Func<TValueIn, TEntity> CreateIfNotFound { get; set; }
        public Func<TValueIn, TEntity, TValueOut> ResultSelector { get; set; }
        // public Expression<Func<TEntity, bool>> WhereClause { get; set; }
        // public Func<IncludableQueryable<TEntity>, IncludableQueryable<TEntity>> Includer { get; set; } = null;
        public Func<DbContextWrapper, IQueryable<TEntity>> Query { get; set; }
        public int CacheSize { get; set; } = 1000;
        public bool GetFullDataset { get; set; } = true;
        public string KeyedConnection { get; set; } = null;
    }
    public class EfCoreLookupStreamNode<TIn, TValue, TEntity, TValueOut, TKey, TOut>(string name, EfCoreLookupArgs<TIn, TValue, TEntity, TValueOut, TKey, TOut> args)
        : StreamNodeBase<TOut, IStream<TOut>, EfCoreLookupArgs<TIn, TValue, TEntity, TValueOut, TKey, TOut>>(name, args)
    {
        private readonly Guid _nodeGuid = Guid.NewGuid();

        public override ProcessImpact PerformanceImpact => Args.GetFullDataset ? ProcessImpact.Light : ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => Args.GetFullDataset ? ProcessImpact.Average : ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(EfCoreLookupArgs<TIn, TValue, TEntity, TValueOut, TKey, TOut> args)
        {
            IPushObservable<TOut> matchingS = args.SourceStream.Observable.Map(elt =>
                {
                    var memoryCache = this.ExecutionContext.Services.GetRequiredService<IMemoryCache>();

                    var matcher = memoryCache.GetOrCreate($"{this.NodeName}-{_nodeGuid}", _ =>
                    {
                        return new EfMatcher<TValue, TEntity, TKey>(new EfMatcherConfig<TValue, TEntity, TKey>
                        {
                            CreateIfNotFound = args.CreateIfNotFound,
                            LeftKeyExpression = args.GetLeftStreamKey,
                            RightKeyExpression = args.GetEntityStreamKey,
                            MinCacheSize = args.CacheSize,
                            GetFullDataset = args.GetFullDataset,
                            GetQuery = (db) => args.Query(new DbContextWrapper(db))
                        });
                    }) ?? throw new InvalidOperationException("Memory cache failure to get or create the matcher");
                    using var ctx = this.ExecutionContext.Services.GetDbContext(args.KeyedConnection);
                    TEntity? entity = default;
                    var val = args.GetInputValue(elt);
                    entity = matcher.GetMatch(val, ctx);
                    return args.GetOutputValue(elt, args.ResultSelector(val, entity));
                });
            return base.CreateUnsortedStream(matchingS);
        }
    }
}