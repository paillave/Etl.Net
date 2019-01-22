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
    public class LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut>
        where TCtx : DbContext
        where TEntity : class
    {
        public IStream<TIn> InputStream { get; set; }
        public ISingleStream<TCtx> DbContextStream { get; set; }
        public Expression<Func<TIn, TEntity, bool>> Match { get; set; }
        public Func<TIn, TEntity> CreateIfNotFound { get; set; }
        public Func<TIn, TEntity, TOut> ResultSelector { get; set; }
        public int CacheSize { get; set; } = 1000;
        public Expression<Func<TEntity, bool>> DefaultCache { get; set; }
    }
    public class LookupEntityFrameworkCoreStreamNode<TIn, TEntity, TCtx, TOut> : StreamNodeBase<TOut, IStream<TOut>, LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut>>
        where TCtx : DbContext
        where TEntity : class
    {
        public LookupEntityFrameworkCoreStreamNode(string name, LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(LookupEntityFrameworkCoreArgs<TIn, TEntity, TCtx, TOut> args)
        {
            var matchingS = args.InputStream.Observable.CombineWithLatest(args.DbContextStream.Observable.Map(ctx =>
            {
                if (args.DefaultCache == null)
                    return new EfMatcher<TIn, TEntity, TCtx>(ctx, args.Match, args.CreateIfNotFound, args.CacheSize);
                else
                    return new EfMatcher<TIn, TEntity, TCtx>(ctx, args.Match, args.CreateIfNotFound, args.DefaultCache, args.CacheSize);
            }), (elt, matcher) =>new { Element = elt, Matcher = matcher }, true)
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