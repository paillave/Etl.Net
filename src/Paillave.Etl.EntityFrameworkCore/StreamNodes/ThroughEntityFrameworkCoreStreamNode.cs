using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using EFCore.BulkExtensions;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using Paillave.Etl.EntityFrameworkCore.Core;

namespace Paillave.Etl.EntityFrameworkCore.StreamNodes
{
    public class ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut>
        where TInEf : class
        where TCtx : DbContext
    {
        public IStream<TIn> SourceStream { get; set; }
        public IStream<TCtx> DbContextStream { get; set; }
        public int BatchSize { get; set; } = 1000;
        public SaveMode BulkLoadMode { get; set; } = SaveMode.BulkUpsert;
        public Expression<Func<TInEf, TInEf, bool>> Compare { get; set; }
        public Func<TIn, TInEf> GetEntity { get; set; }
        public Func<TIn, TInEf, TOut> GetOutput { get; set; }
    }
    public class ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TKey, TIn, TOut>
        where TInEf : class
        where TCtx : DbContext
    {
        public IStream<TIn> SourceStream { get; set; }
        public IStream<TCtx> DbContextStream { get; set; }
        public int BatchSize { get; set; } = 1000;
        public Expression<Func<TInEf, TKey>> GetKey { get; set; }
        public Func<TIn, TInEf> GetEntity { get; set; }
        public Func<TIn, TInEf, TOut> GetOutput { get; set; }
        public SaveByKeyMode BulkLoadMode { get; set; } = SaveByKeyMode.BulkUpsert;
    }
    public enum SaveMode
    {
        StandardEfCoreUpsert,
        BulkInsert,
        BulkUpsert
    }
    public enum SaveByKeyMode
    {
        BulkInsert,
        BulkUpsert
    }
    public class ThroughEntityFrameworkCoreStreamNode<TInEf, TCtx, TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut>>
        where TInEf : class
        where TCtx : DbContext
    {
        public ThroughEntityFrameworkCoreStreamNode(string name, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut> args) : base(name, args)
        {
            if (args.Compare != null)
                args.BulkLoadMode = SaveMode.StandardEfCoreUpsert;
        }

        protected override IStream<TOut> CreateOutputStream(ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut> args)
        {
            var dbContextStream = args.DbContextStream.Observable.First();
            var ret = args.SourceStream.Observable
                .Chunk(args.BatchSize)
                .CombineWithLatest(dbContextStream, (i, c) => new { Context = c, Items = i.Select(j => new Tuple<TIn, TInEf>(j, args.GetEntity(j))).ToList() }, true)
                .Do(i => ProcessBatch(i.Items, i.Context, args.BulkLoadMode))
                .FlatMap(i => PushObservable.FromEnumerable(i.Items))
                .Map(i => args.GetOutput(i.Item1, i.Item2));
            return base.CreateUnsortedStream(ret);
        }
        public void ProcessBatch(List<Tuple<TIn, TInEf>> items, TCtx dbContext, SaveMode bulkLoadMode)
        {
            var entities = items.Select(i => i.Item2).ToArray();
            switch (bulkLoadMode)
            {
                case SaveMode.StandardEfCoreUpsert:
                    if (Args.Compare != null)
                    {
                        var entityType = dbContext.Model.GetEntityTypes().FirstOrDefault(i => string.Equals(i.Name.Split('.').Last(), typeof(TInEf).Name, StringComparison.InvariantCultureIgnoreCase));
                        var keyPropertyInfos = entityType.GetProperties().Where(i => !i.IsShadowProperty).Where(i => i.IsPrimaryKey()).Select(i => i.PropertyInfo).ToList();
                        foreach (var entity in entities)
                        {
                            var expr = Args.Compare.ApplyPartialLeft(entity);
                            TInEf elt = dbContext.Set<TInEf>().AsNoTracking().FirstOrDefault(expr);
                            if (elt != null)
                            {
                                foreach (var keyPropertyInfo in keyPropertyInfos)
                                {
                                    object val = keyPropertyInfo.GetValue(elt);
                                    keyPropertyInfo.SetValue(entity, val);
                                }
                            }
                        }
                    }
                    dbContext.UpdateRange(entities);
                    break;
                case SaveMode.BulkInsert:
                    dbContext.BulkInsert(entities, new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true, BatchSize = items.Count });
                    break;
                case SaveMode.BulkUpsert:
                    dbContext.BulkInsertOrUpdate(entities, new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true, BatchSize = items.Count });
                    break;
                default:
                    break;
            }
            dbContext.SaveChanges(); //DO NOT SET IT ASYNC HERE. The point is to retrieve the Id in case of automatic key.
        }
    }
    public class ThroughEntityFrameworkCoreStreamNode<TInEf, TCtx, TKey, TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TKey, TIn, TOut>>
        where TInEf : class
        where TCtx : DbContext
    {
        private List<string> _keyProperties = new List<string>();

        // private SingleKeyBulkUpserter<TInEf, TCtx, TKey> _bulkUpserter;
        public ThroughEntityFrameworkCoreStreamNode(string name, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TKey, TIn, TOut> args) : base(name, args)
        {
            _keyProperties = new KeyDefinitionExtractor().GetKeys(args.GetKey).Select(i => i.Name).ToList();
            // _bulkUpserter = new SingleKeyBulkUpserter<TInEf, TCtx, TKey>(args.GetKey);
        }

        protected override IStream<TOut> CreateOutputStream(ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TKey, TIn, TOut> args)
        {
            var dbContextStream = args.DbContextStream.Observable.First();
            var ret = args.SourceStream.Observable
                .Chunk(args.BatchSize)
                .CombineWithLatest(dbContextStream, (i, c) => new { Context = c, Items = i.Select(j => new Tuple<TIn, TInEf>(j, args.GetEntity(j))).ToList() }, true)
                .Do(i => ProcessBatch(i.Items, i.Context, args.BulkLoadMode))
                .FlatMap(i => PushObservable.FromEnumerable(i.Items))
                .Map(i => args.GetOutput(i.Item1, i.Item2));
            return base.CreateUnsortedStream(ret);
        }
        public void ProcessBatch(List<Tuple<TIn, TInEf>> items, TCtx dbContext, SaveByKeyMode bulkLoadMode)
        {
            var bulkConfig = new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true, BatchSize = items.Count, UpdateByProperties = _keyProperties };
            switch (bulkLoadMode)
            {
                case SaveByKeyMode.BulkInsert:
                    dbContext.BulkInsert(items.Select(i => i.Item2).ToList(), bulkConfig);
                    break;
                case SaveByKeyMode.BulkUpsert:
                    dbContext.BulkInsertOrUpdate(items.Select(i => i.Item2).ToList(), bulkConfig);
                    break;
                default:
                    break;
            }
            dbContext.SaveChanges(); //DO NOT SET IT ASYNC HERE. The point is to retrieve the Id in case of automatic key.
        }
    }
}
