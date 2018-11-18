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
        public SaveMode BulkLoadMode { get; set; } = SaveMode.BulkInsert;
        public Func<TIn, TInEf> GetEntity { get; set;}
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
    }
    public enum SaveMode
    {
        StandardEfCoreUpsert,
        BulkInsert,
        BulkUpsert
    }
    public class ThroughEntityFrameworkCoreStreamNode<TInEf, TCtx, TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut>>
        where TInEf : class
        where TCtx : DbContext
    {
        public override bool IsAwaitable => true;
        public ThroughEntityFrameworkCoreStreamNode(string name, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TIn, TOut> args) : base(name, args)
        {
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
        public override bool IsAwaitable => true;
        private BulkUpserter<TInEf, TCtx, TKey> _bulkUpserter;
        public ThroughEntityFrameworkCoreStreamNode(string name, ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TKey, TIn, TOut> args) : base(name, args)
        {
            _bulkUpserter = new BulkUpserter<TInEf, TCtx, TKey>(args.GetKey);
        }

        protected override IStream<TOut> CreateOutputStream(ThroughEntityFrameworkCoreArgs<TInEf, TCtx, TKey, TIn, TOut> args)
        {
            var dbContextStream = args.DbContextStream.Observable.First();
            var ret = args.SourceStream.Observable
                .Chunk(args.BatchSize)
                .CombineWithLatest(dbContextStream, (i, c) => new { Context = c, Items = i.Select(j => new Tuple<TIn, TInEf>(j, args.GetEntity(j))).ToList() }, true)
                .Do(i => ProcessBatch(i.Items, i.Context))
                .FlatMap(i => PushObservable.FromEnumerable(i.Items))
                .Map(i => args.GetOutput(i.Item1, i.Item2));
            return base.CreateUnsortedStream(ret);
        }
        public void ProcessBatch(List<Tuple<TIn, TInEf>> items, TCtx dbContext)
        {
            _bulkUpserter.ProcessBatch(items.Select(i => i.Item2).ToList(), dbContext);
        }
    }
}
