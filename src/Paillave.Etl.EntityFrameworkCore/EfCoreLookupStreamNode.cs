using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using Microsoft.EntityFrameworkCore;
using Paillave.EntityFrameworkCoreExtension.Core;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.EntityFrameworkCore
{
    #region Non correlated args builder
    public class EfCoreLookupArgsBuilder<TIn>
    {
        internal IStream<TIn> SourceStream { get; }
        internal EfCoreLookupArgsBuilder(IStream<TIn> sourceStream) => (SourceStream) = (sourceStream);
        public EfCoreLookupArgsBuilder<TIn, TEntity> Set<TEntity>(string keyedConnection = null) where TEntity : class => new EfCoreLookupArgsBuilder<TIn, TEntity>(this, keyedConnection);
    }
    public class EfCoreLookupArgsBuilder<TIn, TEntity> where TEntity : class
    {
        internal EfCoreLookupArgsBuilder<TIn> Parent { get; }
        internal Expression<Func<TEntity, bool>> WhereClause { get; private set; } = null;
        internal Func<IncludableQueryable<TEntity>, IncludableQueryable<TEntity>> IncludeClause { get; private set; } = null;
        internal string KeyedConnection { get; private set; }
        internal EfCoreLookupArgsBuilder(EfCoreLookupArgsBuilder<TIn> parent, string keyedConnection = null) => (Parent, KeyedConnection) = (parent, keyedConnection);
        public EfCoreLookupArgsBuilder<TIn, TEntity> Where(Expression<Func<TEntity, bool>> where)
        {
            this.WhereClause = where;
            return this;
        }
        public EfCoreLookupArgsBuilder<TIn, TEntity> Include(Func<IncludableQueryable<TEntity>, IncludableQueryable<TEntity>> includeClause)
        {
            this.IncludeClause = includeClause;
            return this;
        }
        public EfCoreLookupArgsBuilder<TIn, TEntity, TKey> On<TKey>(Expression<Func<TIn, TKey>> getLeftStreamKey, Expression<Func<TEntity, TKey>> getEntityStreamKey)
            => new EfCoreLookupArgsBuilder<TIn, TEntity, TKey>(this, getLeftStreamKey, getEntityStreamKey);
    }
    public class EfCoreLookupArgsBuilder<TIn, TEntity, TKey> where TEntity : class
    {
        internal EfCoreLookupArgsBuilder<TIn, TEntity> Parent { get; }
        internal Expression<Func<TIn, TKey>> GetLeftStreamKey { get; }
        internal Expression<Func<TEntity, TKey>> GetEntityStreamKey { get; }
        internal EfCoreLookupArgsBuilder(EfCoreLookupArgsBuilder<TIn, TEntity> parent, Expression<Func<TIn, TKey>> getLeftStreamKey, Expression<Func<TEntity, TKey>> getEntityStreamKey)
            => (Parent, GetLeftStreamKey, GetEntityStreamKey) = (parent, getLeftStreamKey, getEntityStreamKey);
        public EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey> Select<TOut>(Func<TIn, TEntity, TOut> resultSelector)
            => new EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey>(this, resultSelector);
    }
    public class EfCoreLookupArgsBuilder<TIn, TEntity, TOut, TKey> where TEntity : class
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
                WhereClause = _parent.Parent.WhereClause,
                Includer = _parent.Parent.IncludeClause,
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
        internal EfCoreLookupCorrelatedArgsBuilder(IStream<Correlated<TIn>> sourceStream) => (SourceStream) = (sourceStream);
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> Set<TEntity>(string keyedConnection = null) where TEntity : class => new EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity>(this, keyedConnection);
    }
    public class EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> where TEntity : class
    {
        internal EfCoreLookupCorrelatedArgsBuilder<TIn> Parent { get; }
        internal Expression<Func<TEntity, bool>> WhereClause { get; private set; } = null;
        internal string KeyedConnection { get; private set; } = null;
        internal Func<IncludableQueryable<TEntity>, IncludableQueryable<TEntity>> IncludeClause { get; private set; } = null;
        internal EfCoreLookupCorrelatedArgsBuilder(EfCoreLookupCorrelatedArgsBuilder<TIn> parent, string keyedConnection) => (Parent, KeyedConnection) = (parent, keyedConnection);
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> Where(Expression<Func<TEntity, bool>> where)
        {
            this.WhereClause = where;
            return this;
        }
        public EfCoreLookupCorrelatedArgsBuilder<TIn, TEntity> Include(Func<IncludableQueryable<TEntity>, IncludableQueryable<TEntity>> includeClause)
        {
            this.IncludeClause = includeClause;
            return this;
        }
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
                WhereClause = _parent.Parent.WhereClause,
                Includer = _parent.Parent.IncludeClause,
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
    public class EfCoreLookupArgs<TIn, TValueIn, TEntity, TValueOut, TKey, TOut> where TEntity : class
    {
        public Func<TIn, TValueIn> GetInputValue { get; set; }
        // public Func<TValueTIn, bool> Where { get; set; }
        public Func<TIn, TValueOut, TOut> GetOutputValue { get; set; }
        public IStream<TIn> SourceStream { get; set; }
        public Expression<Func<TValueIn, TKey>> GetLeftStreamKey { get; set; }
        public Expression<Func<TEntity, TKey>> GetEntityStreamKey { get; set; }
        public Func<TValueIn, TEntity> CreateIfNotFound { get; set; }
        public Func<TValueIn, TEntity, TValueOut> ResultSelector { get; set; }
        public Expression<Func<TEntity, bool>> WhereClause { get; set; }
        public Func<IncludableQueryable<TEntity>, IncludableQueryable<TEntity>> Includer { get; set; } = null;
        public int CacheSize { get; set; } = 1000;
        public bool GetFullDataset { get; set; } = true;
        public string KeyedConnection { get; set; } = null;
    }
    public class EfCoreLookupStreamNode<TIn, TValue, TEntity, TValueOut, TKey, TOut> : StreamNodeBase<TOut, IStream<TOut>, EfCoreLookupArgs<TIn, TValue, TEntity, TValueOut, TKey, TOut>>
        where TEntity : class
    {
        public EfCoreLookupStreamNode(string name, EfCoreLookupArgs<TIn, TValue, TEntity, TValueOut, TKey, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => Args.GetFullDataset ? ProcessImpact.Light : ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => Args.GetFullDataset ? ProcessImpact.Average : ProcessImpact.Light;

        protected override IStream<TOut> CreateOutputStream(EfCoreLookupArgs<TIn, TValue, TEntity, TValueOut, TKey, TOut> args)
        {
            IPushObservable<TOut> matchingS = args.SourceStream.Observable.Map(elt =>
                    {
                        var matcher = this.ExecutionContext.ContextBag.Resolve(this.NodeName, () =>
                        {
                            var ctx = args.KeyedConnection == null
                                ? this.ExecutionContext.DependencyResolver.Resolve<DbContext>()
                                : this.ExecutionContext.DependencyResolver.Resolve<DbContext>(args.KeyedConnection);
                            return this.ExecutionContext.InvokeInDedicatedThread(ctx, () => new EfMatcher<TValue, TEntity, TKey>(new EfMatcherConfig<TValue, TEntity, TKey>
                            {
                                Context = ctx,
                                CreateIfNotFound = args.CreateIfNotFound,
                                WhereClause = args.WhereClause,
                                LeftKeyExpression = args.GetLeftStreamKey,
                                RightKeyExpression = args.GetEntityStreamKey,
                                MinCacheSize = args.CacheSize,
                                GetFullDataset = args.GetFullDataset,
                                IncludeClause = i =>
                                {
                                    if (args.Includer == null)
                                        return i;
                                    else
                                        return args.Includer(new IncludableQueryable<TEntity>(i)).Queryable;
                                }
                            }));
                        });
                        TEntity entity = default;
                        var val = args.GetInputValue(elt);
                        if (!args.GetFullDataset)
                        {
                            entity = this.ExecutionContext.InvokeInDedicatedThread(matcher.Context, () => matcher.GetMatch(val));
                        }
                        else
                        {
                            entity = matcher.GetMatch(val);
                        }
                        return args.GetOutputValue(elt, args.ResultSelector(val, entity));
                    }
                );

            return base.CreateUnsortedStream(matchingS);
        }
    }
}