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
        public Func<TIn, TEntity, TOut> ResultSelector { get; set; }
        public int CacheSize { get; set; } = 1000;
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
            var matcher = new EfMatcher<TIn, TEntity, TCtx>(args.Match, args.CacheSize);
            var matchingS = args.InputStream.Observable.CombineWithLatest(args.DbContextStream.Observable, (elt, ctx) => new { Element = elt, DbContext = ctx }, true)
            .Map(i => args.ResultSelector(i.Element, matcher.GetMatch(i.DbContext, i.Element)));
            return base.CreateUnsortedStream(matchingS);
        }
    }
}