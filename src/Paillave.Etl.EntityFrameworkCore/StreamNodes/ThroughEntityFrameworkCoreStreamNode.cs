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

namespace Paillave.Etl.EntityFrameworkCore.StreamNodes
{
    public class ThroughEntityFrameworkCoreArgs<TIn, TCtx, TStream>
        where TIn : class
        where TStream : IStream<TIn>
        where TCtx : DbContext
    {
        public TStream SourceStream { get; set; }
        public IStream<TCtx> DbContextStream { get; set; }
        public int BatchSize { get; set; } = 1000;
        public SaveMode BulkLoadMode { get; set; } = SaveMode.BulkInsert;
    }
    public enum SaveMode
    {
        StandardEfCoreUpsert,
        BulkInsert,
        BulkUpsert
    }
    public class ThroughEntityFrameworkCoreStreamNode<TIn, TCtx, TStream> : StreamNodeBase<TIn, TStream, ThroughEntityFrameworkCoreArgs<TIn, TCtx, TStream>>
        where TIn : class
        where TStream : IStream<TIn>
        where TCtx : DbContext
    {
        public override bool IsAwaitable => true;
        public ThroughEntityFrameworkCoreStreamNode(string name, ThroughEntityFrameworkCoreArgs<TIn, TCtx, TStream> args) : base(name, args)
        {
        }

        protected override TStream CreateOutputStream(ThroughEntityFrameworkCoreArgs<TIn, TCtx, TStream> args)
        {
            var dbContextStream = args.DbContextStream.Observable.First();
            var ret = args.SourceStream.Observable
                .Chunk(args.BatchSize)
                .CombineWithLatest(dbContextStream, (i, c) => new { Context = c, Items = i }, true)
                .Do(i => ProcessBatch(i.Items.ToList(), i.Context, args.BulkLoadMode))
                .FlatMap(i => PushObservable.FromEnumerable(i.Items));
            return base.CreateMatchingStream(ret, args.SourceStream);
        }
        public void ProcessBatch(List<TIn> items, TCtx dbContext, SaveMode bulkLoadMode)
        {
            switch (bulkLoadMode)
            {
                case SaveMode.StandardEfCoreUpsert:
                    dbContext.UpdateRange(items);
                    break;
                case SaveMode.BulkInsert:
                    dbContext.BulkInsert(items, new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true, BatchSize = items.Count });
                    break;
                case SaveMode.BulkUpsert:
                    dbContext.BulkInsertOrUpdate(items, new BulkConfig { PreserveInsertOrder = true, SetOutputIdentity = true, BatchSize = items.Count });
                    break;
                default:
                    break;
            }
            dbContext.SaveChanges(); //DO NOT SET IT ASYNC HERE. The point is to retrieve the Id in case of automatic key.
        }
    }
}
