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

namespace Paillave.Etl.EntityFrameworkCore.StreamNodes
{
    public class ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut>
        where TInEf : class
        where TCtx : DbContext
    {
        public IStream<TIn> SourceStream { get; set; }
        public ISingleStream<TCtx> DbContextStream { get; set; }
        public int BatchSize { get; set; } = 10000;
        public SaveMode BulkLoadMode { get; set; } = SaveMode.Bulk;
        public Func<TIn, TCtx, TInEf> GetEntity { get; set; }
        public Func<TIn, TInEf, TOut> GetOutput { get; set; }
        public Expression<Func<TInEf, object>> PivotKey { get; set; }
    }
    public enum SaveMode
    {
        //EntityFrameworkCore,
        Bulk
    }
    public class ThroughEntityFrameworkCoreStreamNode<TInEf, TCtx, TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut>>
        where TInEf : class
        where TCtx : DbContext
    {
        public ThroughEntityFrameworkCoreStreamNode(string name, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut> args) : base(name, args)
        {
            //if (args.Compare != null)
            //    args.BulkLoadMode = SaveMode.StandardEfCoreUpsert;
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
            //var bulkConfig = new BulkConfig
            //{
            //    SetOutputIdentity = true,
            //    BatchSize = items.Count,
            //    TrackingEntities = false
            //};
            switch (bulkLoadMode)
            {
                //case SaveMode.StandardEfCoreUpsert:
                //    if (Args.Compare != null)
                //    {
                //        var entityType = dbContext.Model.GetEntityTypes().FirstOrDefault(i => string.Equals(i.Name.Split('.').Last(), typeof(TInEf).Name, StringComparison.InvariantCultureIgnoreCase));
                //        var keyPropertyInfos = entityType.GetProperties().Where(i => !i.IsShadowProperty).Where(i => i.IsPrimaryKey()).Select(i => i.PropertyInfo).ToList();
                //        foreach (var entity in entities)
                //        {
                //            var expr = Args.Compare.ApplyPartialLeft(entity);
                //            TInEf elt = dbContext.Set<TInEf>().AsNoTracking().FirstOrDefault(expr);
                //            if (elt != null)
                //            {
                //                foreach (var keyPropertyInfo in keyPropertyInfos)
                //                {
                //                    object val = keyPropertyInfo.GetValue(elt);
                //                    keyPropertyInfo.SetValue(entity, val);
                //                }
                //            }
                //        }
                //    }
                //    dbContext.UpdateRange(entities);
                //    break;
                case SaveMode.Bulk:
                    dbContext.BulkSave(entities, Args.PivotKey);
                    break;
                default:
                    break;
            }
            DetachAllEntities(dbContext);
            //dbContext.SaveChanges(); //DO NOT SET IT ASYNC HERE. The point is to retrieve the Id in case of automatic key.
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
