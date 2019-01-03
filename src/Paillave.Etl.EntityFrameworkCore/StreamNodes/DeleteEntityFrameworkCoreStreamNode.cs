using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.EntityFrameworkCore.Core;
using Microsoft.EntityFrameworkCore;
using Paillave.Etl.EntityFrameworkCore.Core;

namespace Paillave.Etl.EntityFrameworkCore.StreamNodes
{
    public class DeleteEntityFrameworkCoreArgs<TIn, TEntity, TCtx>
        where TCtx : DbContext
        where TEntity : class
    {
        public IStream<TIn> InputStream { get; set; }
        public ISingleStream<TCtx> DbContextStream { get; set; }
        public Expression<Func<TIn, TEntity, bool>> Match { get; set; }
    }
    public class DeleteEntityFrameworkCoreStreamNode<TIn, TEntity, TCtx> : StreamNodeBase<TIn, IStream<TIn>, DeleteEntityFrameworkCoreArgs<TIn, TEntity, TCtx>>
        where TCtx : DbContext
        where TEntity : class
    {
        public DeleteEntityFrameworkCoreStreamNode(string name, DeleteEntityFrameworkCoreArgs<TIn, TEntity, TCtx> args) : base(name, args)
        {
        }

        protected override IStream<TIn> CreateOutputStream(DeleteEntityFrameworkCoreArgs<TIn, TEntity, TCtx> args)
        {
            var matchingS = args.InputStream.Observable
                .CombineWithLatest(args.DbContextStream.Observable, (elt, ctx) => new { Element = elt, DbContext = ctx }, true)
                .Map(i =>
                {
                    i.DbContext.DeleteWhere<TEntity>(args.Match.ApplyPartialLeft<TIn, TEntity, bool>(i.Element));
                    i.DbContext.SaveChanges();
                    return i.Element;
                });
            return base.CreateUnsortedStream(matchingS);
        }
    }
}