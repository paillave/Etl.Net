using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using System.Linq.Expressions;
using Paillave.EntityFrameworkCoreExtension.BulkSave;

namespace Paillave.Etl.EntityFrameworkCore
{
    public class UpdateEntityFrameworkCoreArgs<TEntity, TSource>
        where TEntity : class
    {
        public IStream<TSource> SourceStream { get; set; }
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
    public class UpdateEntityFrameworkCoreStreamNode<TEntity, TSource> : StreamNodeBase<TSource, IStream<TSource>, UpdateEntityFrameworkCoreArgs<TEntity, TSource>>
        where TEntity : class
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public UpdateEntityFrameworkCoreStreamNode(string name, UpdateEntityFrameworkCoreArgs<TEntity, TSource> args) : base(name, args)
        {
        }

        protected override IStream<TSource> CreateOutputStream(UpdateEntityFrameworkCoreArgs<TEntity, TSource> args)
        {
            var ret = args.SourceStream.Observable
                .Chunk(args.BatchSize)
                .Do(i =>
                {
                    var dbContext = this.ExecutionContext.DependencyResolver.Resolve<DbContext>();
                    this.ExecutionContext.InvokeInDedicatedThreadAsync(dbContext, () => ProcessBatch(i.ToList(), dbContext, args.BulkLoadMode)).Wait();
                })
                .FlatMap((i, ct) => PushObservable.FromEnumerable(i, ct));
            return base.CreateUnsortedStream(ret);
        }
        public void ProcessBatch(List<TSource> sources, DbContext dbContext, UpdateMode bulkLoadMode)
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
