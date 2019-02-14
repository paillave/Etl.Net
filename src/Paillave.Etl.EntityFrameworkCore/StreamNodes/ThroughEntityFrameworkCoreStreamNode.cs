using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.EntityFrameworkCore.BulkSave;
using Paillave.Etl.EntityFrameworkCore.EfSave;

namespace Paillave.Etl.EntityFrameworkCore.StreamNodes
{
    public class ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut>
        where TInEf : class
        where TCtx : DbContext
    {
        public IStream<TIn> SourceStream { get; set; }
        public ISingleStream<TCtx> DbContextStream { get; set; }
        public int BatchSize { get; set; } = 10000;
        public SaveMode BulkLoadMode { get; set; } = SaveMode.SqlServerBulk;
        public Func<TIn, TCtx, TInEf> GetEntity { get; set; }
        public Func<TIn, TInEf, TOut> GetOutput { get; set; }
        public Expression<Func<TInEf, object>> PivotKey { get; set; }
    }
    public enum SaveMode
    {
        EntityFrameworkCore,
        SqlServerBulk
    }
    public class ThroughEntityFrameworkCoreStreamNode<TInEf, TCtx, TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut>>
        where TInEf : class
        where TCtx : DbContext
    {
        public ThroughEntityFrameworkCoreStreamNode(string name, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut> args)
        {
            var dbContextStream = args.DbContextStream.Observable.First();
            var ret = args.SourceStream.Observable
                .Chunk(args.BatchSize)
                .CombineWithLatest(dbContextStream, (i, c) => new { Context = c, Items = i.Select(j => new Tuple<TIn, TInEf>(j, args.GetEntity(j, c))).ToList() }, true)
                .Do(i => this.ExecutionContext.InvokeInDedicatedThread(i.Context, () => ProcessBatch(i.Items, i.Context, args.BulkLoadMode)))
                .FlatMap(i => PushObservable.FromEnumerable(i.Items))
                .Map(i => args.GetOutput(i.Item1, i.Item2));
            return base.CreateUnsortedStream(ret);
        }
        public void ProcessBatch(List<Tuple<TIn, TInEf>> items, TCtx dbContext, SaveMode bulkLoadMode)
        {
            var entities = items.Select(i => i.Item2).ToArray();
            switch (bulkLoadMode)
            {
                case SaveMode.EntityFrameworkCore:
                    dbContext.EfSave(entities, Args.PivotKey);
                    break;
                case SaveMode.SqlServerBulk:
                    dbContext.BulkSave(entities, Args.PivotKey);
                    break;
                default:
                    break;
            }
            DetachAllEntities(dbContext);
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
