using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.EntityFrameworkCore.Core;
using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.EntityFrameworkCore.StreamNodes
{
    public class LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut, TKey>
        where TCtx : DbContext
        where TEntity : class
    {
        public IStream<TIn> InputStream { get; set; }
        public ISingleStream<TCtx> DbContextStream { get; set; }
        public Expression<Func<TIn, TKey>> GetLeftStreamKey { get; set; }
        public Expression<Func<TEntity, TKey>> GetEntityStreamKey { get; set; }
        public Func<TIn, TCtx, TEntity> CreateIfNotFound { get; set; }
        public Func<TIn, TEntity, TOut> ResultSelector { get; set; }
        public int CacheSize { get; set; } = 1000;
        public Expression<Func<TEntity, bool>> DefaultCriteria { get; set; }
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> IncludeInstruction { get; set; } = null;
        public bool GetFullDataset { get; set; } = false;
    }
    public class LookupEntityFrameworkCoreStreamNode<TIn, TEntity, TCtx, TOut, TKey> : StreamNodeBase<TOut, IStream<TOut>, LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut, TKey>>
        where TCtx : DbContext
        where TEntity : class
    {
        public LookupEntityFrameworkCoreStreamNode(string name, LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut, TKey> args) : base(name, args)
        {
        }
        protected override IStream<TOut> CreateOutputStream(LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut, TKey> args)
        {
            var matchingS = args.InputStream.Observable.CombineWithLatest(args.DbContextStream.Observable.Map(ctx =>
            {
                return new EfMatcher<TIn, TEntity, TCtx, TKey>(new EfMatcherConfig<TIn, TEntity, TCtx, TKey>
                {
                    Context = ctx,
                    CreateIfNotFound = args.CreateIfNotFound,
                    DefaultDatasetCriteria = args.DefaultCriteria,
                    LeftKeyExpression = args.GetLeftStreamKey,
                    RightKeyExpression = args.GetEntityStreamKey,
                    MinCacheSize = args.CacheSize,
                    GetFullDataset = args.GetFullDataset,
                    IncludeInstruction = args.IncludeInstruction
                });
            }), (elt, matcher) => new { Element = elt, Matcher = matcher }, true)
            .Map(i =>
            {
                TEntity entity = default(TEntity);
                this.ExecutionContext.InvokeInDedicatedThread(i.Matcher.Context, () => entity = i.Matcher.GetMatch(i.Element));
                return args.ResultSelector(i.Element, entity);
            });
            return base.CreateUnsortedStream(matchingS);
        }
    }
}