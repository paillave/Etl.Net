using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Paillave.Etl.EntityFrameworkCore.Core;
using Paillave.Etl.EntityFrameworkCore.BulkSave;
using Paillave.Etl.EntityFrameworkCore.EfSave;

namespace Paillave.Etl.EntityFrameworkCore.StreamNodes
{
    public class UpdateEntityFrameworkCoreArgs<TEntity, TCtx, TSource>
        where TEntity : class
        where TCtx : DbContext
    {
        public IStream<TSource> SourceStream { get; set; }
        public ISingleStream<TCtx> DbContextStream { get; set; }
        public int BatchSize { get; set; } = 10000;
        public UpdateMode BulkLoadMode { get; set; } = UpdateMode.SqlServerBulk;
        public Expression<Func<TSource, TEntity>> UpdateKey { get; set; }
        public Expression<Func<TSource, TEntity>> UpdateValues { get; set; }
    }
    public enum UpdateMode
    {
        //EntityFrameworkCore,
        SqlServerBulk
    }
    public class UpdateEntityFrameworkCoreStreamNode<TEntity, TCtx, TSource> : StreamNodeBase<TSource, IStream<TSource>, UpdateEntityFrameworkCoreArgs<TEntity, TCtx, TSource>>
        where TEntity : class
        where TCtx : DbContext
    {
        public UpdateEntityFrameworkCoreStreamNode(string name, UpdateEntityFrameworkCoreArgs<TEntity, TCtx, TSource> args) : base(name, args)
        {
        }

        protected override IStream<TSource> CreateOutputStream(UpdateEntityFrameworkCoreArgs<TEntity, TCtx, TSource> args)
        {
            var dbContextStream = args.DbContextStream.Observable.First();
            var ret = args.SourceStream.Observable
                .Chunk(args.BatchSize)
                .CombineWithLatest(dbContextStream, (i, c) => new { Context = c, Items = i.ToList() }, true)
                .Do(i => this.ExecutionContext.InvokeInDedicatedThread(i.Context, () => ProcessBatch(i.Items, i.Context, args.BulkLoadMode)))
                .FlatMap(i => PushObservable.FromEnumerable(i.Items));
            return base.CreateUnsortedStream(ret);
        }
        public void ProcessBatch(List<TSource> sources, TCtx dbContext, UpdateMode bulkLoadMode)
        {
            switch (bulkLoadMode)
            {
                //case UpdateMode.EntityFrameworkCore:
                //    dbContext.EfUpdate(entities, Args.PivotKey);
                //    DetachAllEntities(dbContext);
                //    break;
                case UpdateMode.SqlServerBulk:
                    dbContext.BulkUpdate<TEntity, TSource>(sources, Args.UpdateKey, Args.UpdateValues);
                    break;
                default:
                    break;
            }
        }
        public void DetachAllEntities(DbContext dbContext)
        {
            var changedEntriesCopy = dbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted)
                .ToList();

            foreach (var entry in changedEntriesCopy)
                entry.State = EntityState.Detached;
        }
    }
}
