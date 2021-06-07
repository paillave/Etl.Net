using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using System.Linq.Expressions;
using Paillave.EntityFrameworkCoreExtension.EfSave;
using Paillave.EntityFrameworkCoreExtension.BulkSave;

namespace Paillave.Etl.EntityFrameworkCore
{
    public class EfCoreSaveArgsBuilder<TInEf, TIn, TOut>
    {
        internal EfCoreSaveArgs<TInEf, TIn, TOut> Args { get; set; }
        public EfCoreSaveArgsBuilder(IStream<TIn> sourceStream, Func<TIn, TInEf> getEntity, Func<TIn, TInEf, TOut> getOutput)
        {
            this.Args = new EfCoreSaveArgs<TInEf, TIn, TOut>
            {
                SourceStream = sourceStream,
                GetEntity = getEntity,
                GetOutput = getOutput
            };
        }
        private EfCoreSaveArgsBuilder(EfCoreSaveArgs<TInEf, TIn, TOut> args)
        {
            this.Args = args;
        }
        private TArgs UpdateArgs<TArgs>(TArgs args) where TArgs : IThroughEntityFrameworkCoreArgs<TIn>
        {
            args.BatchSize = this.Args.BatchSize;
            args.BulkLoadMode = this.Args.BulkLoadMode;
            args.DoNotUpdateIfExists = this.Args.DoNotUpdateIfExists;
            args.InsertOnly = this.Args.InsertOnly;
            args.SourceStream = this.Args.SourceStream;
            args.KeyedConnection = this.Args.KeyedConnection;
            return args;
        }
        public EfCoreSaveArgsBuilder<TNewInEf, TIn, TNewInEf> Entity<TNewInEf>(Func<TIn, TNewInEf> getEntity) where TNewInEf : class
            => new EfCoreSaveArgsBuilder<TNewInEf, TIn, TNewInEf>(UpdateArgs(new EfCoreSaveArgs<TNewInEf, TIn, TNewInEf>
            {
                GetEntity = getEntity,
                GetOutput = (i, j) => j
            }));

        public EfCoreSaveArgsBuilder<TInEf, TIn, TOut> SeekOn(Expression<Func<TInEf, object>> pivot)
        {
            this.Args.PivotKeys = new List<Expression<Func<TInEf, object>>> { pivot };
            return this;
        }
        public EfCoreSaveArgsBuilder<TInEf, TIn, TOut> AlternativelySeekOn(Expression<Func<TInEf, object>> pivot)
        {
            this.Args.PivotKeys.Add(pivot);
            return this;
        }
        public EfCoreSaveArgsBuilder<TInEf, TIn, TNewOut> Output<TNewOut>(Func<TIn, TInEf, TNewOut> getOutput)
            => new EfCoreSaveArgsBuilder<TInEf, TIn, TNewOut>(UpdateArgs(new EfCoreSaveArgs<TInEf, TIn, TNewOut>
            {
                GetEntity = this.Args.GetEntity,
                GetOutput = getOutput,
                PivotKeys = this.Args.PivotKeys
            }));

        public EfCoreSaveArgsBuilder<TInEf, TIn, TOut> WithBatchSize(int batchSize)
        {
            this.Args.BatchSize = batchSize;
            return this;
        }
        public EfCoreSaveArgsBuilder<TInEf, TIn, TOut> WithKeyedConnection(string keyedConnection)
        {
            this.Args.KeyedConnection = keyedConnection;
            return this;
        }
        public EfCoreSaveArgsBuilder<TInEf, TIn, TOut> WithMode(SaveMode bulkLoadMode)
        {
            this.Args.BulkLoadMode = bulkLoadMode;
            return this;
        }
        public EfCoreSaveArgsBuilder<TInEf, TIn, TOut> DoNotUpdateIfExists(bool doNotUpdateIfExists = true)
        {
            this.Args.DoNotUpdateIfExists = doNotUpdateIfExists;
            return this;
        }
        public EfCoreSaveArgsBuilder<TInEf, TIn, TOut> InsertOnly(bool insertOnly = true)
        {
            this.Args.InsertOnly = insertOnly;
            return this;
        }
    }







    public class EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut>
    {
        internal EfCoreSaveArgs<TInEf, Correlated<TIn>, Correlated<TOut>> Args { get; set; }
        public EfCoreSaveCorrelatedArgsBuilder(IStream<Correlated<TIn>> sourceStream, Func<TIn, TInEf> getEntity, Func<TIn, TInEf, TOut> getOutput)
        {
            this.Args = new EfCoreSaveArgs<TInEf, Correlated<TIn>, Correlated<TOut>>
            {
                SourceStream = sourceStream,
                GetEntity = i => getEntity(i.Row),
                GetOutput = (i, e) => new Correlated<TOut> { Row = getOutput(i.Row, e), CorrelationKeys = i.CorrelationKeys }
            };
        }
        private EfCoreSaveCorrelatedArgsBuilder(EfCoreSaveArgs<TInEf, Correlated<TIn>, Correlated<TOut>> args)
        {
            this.Args = args;
        }
        private TArgs UpdateArgs<TArgs>(TArgs args) where TArgs : IThroughEntityFrameworkCoreArgs<Correlated<TIn>>
        {
            args.BatchSize = this.Args.BatchSize;
            args.BulkLoadMode = this.Args.BulkLoadMode;
            args.DoNotUpdateIfExists = this.Args.DoNotUpdateIfExists;
            args.InsertOnly = this.Args.InsertOnly;
            args.SourceStream = this.Args.SourceStream;
            args.KeyedConnection = this.Args.KeyedConnection;
            return args;
        }
        public EfCoreSaveCorrelatedArgsBuilder<TNewInEf, TIn, TNewInEf> Entity<TNewInEf>(Func<TIn, TNewInEf> getEntity) where TNewInEf : class
            => new EfCoreSaveCorrelatedArgsBuilder<TNewInEf, TIn, TNewInEf>(UpdateArgs(new EfCoreSaveArgs<TNewInEf, Correlated<TIn>, Correlated<TNewInEf>>
            {
                GetEntity = i => getEntity(i.Row),
                GetOutput = (i, j) => new Correlated<TNewInEf> { Row = j, CorrelationKeys = i.CorrelationKeys }
            }));

        public EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut> SeekOn(Expression<Func<TInEf, object>> pivot)
        {
            this.Args.PivotKeys = new List<Expression<Func<TInEf, object>>> { pivot };
            return this;
        }
        public EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut> AlternativelySeekOn(Expression<Func<TInEf, object>> pivot)
        {
            this.Args.PivotKeys.Add(pivot);
            return this;
        }
        public EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TNewOut> Output<TNewOut>(Func<TIn, TInEf, TNewOut> getOutput)
            => new EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TNewOut>(UpdateArgs(new EfCoreSaveArgs<TInEf, Correlated<TIn>, Correlated<TNewOut>>
            {
                GetEntity = this.Args.GetEntity,
                GetOutput = (i, e) => new Correlated<TNewOut> { Row = getOutput(i.Row, e), CorrelationKeys = i.CorrelationKeys },
                PivotKeys = this.Args.PivotKeys
            }));

        public EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut> WithBatchSize(int batchSize)
        {
            this.Args.BatchSize = batchSize;
            return this;
        }
        public EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut> WithKeyedConnection(string keyedConnection)
        {
            this.Args.KeyedConnection = keyedConnection;
            return this;
        }
        public EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut> WithMode(SaveMode bulkLoadMode)
        {
            this.Args.BulkLoadMode = bulkLoadMode;
            return this;
        }
        public EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut> DoNotUpdateIfExists(bool doNotUpdateIfExists = true)
        {
            this.Args.DoNotUpdateIfExists = doNotUpdateIfExists;
            return this;
        }
        public EfCoreSaveCorrelatedArgsBuilder<TInEf, TIn, TOut> InsertOnly(bool insertOnly = true)
        {
            this.Args.InsertOnly = insertOnly;
            return this;
        }
    }

    internal interface IThroughEntityFrameworkCoreArgs<TIn>
    {
        IStream<TIn> SourceStream { get; set; }
        int BatchSize { get; set; }
        SaveMode BulkLoadMode { get; set; }
        bool DoNotUpdateIfExists { get; set; }
        bool InsertOnly { get; set; }
        string KeyedConnection { get; set; }
    }
    public class EfCoreSaveArgs<TInEf, TIn, TOut> : IThroughEntityFrameworkCoreArgs<TIn>
    {
        internal EfCoreSaveArgs() { }
        public IStream<TIn> SourceStream { get; set; }
        public int BatchSize { get; set; } = 10000;
        public SaveMode BulkLoadMode { get; set; } = SaveMode.SqlServerBulk;
        public Func<TIn, TInEf> GetEntity { get; set; }
        public Func<TIn, TInEf, TOut> GetOutput { get; set; }
        public List<Expression<Func<TInEf, object>>> PivotKeys { get; set; }
        public bool DoNotUpdateIfExists { get; set; } = false;
        public bool InsertOnly { get; set; } = false;
        public string KeyedConnection { get; set; } = null;
    }
    public enum SaveMode
    {
        EntityFrameworkCore,
        SqlServerBulk,
    }
    public class EfCoreSaveStreamNode<TInEf, TIn, TOut> : StreamNodeBase<TOut, IStream<TOut>, EfCoreSaveArgs<TInEf, TIn, TOut>>
        where TInEf : class
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public EfCoreSaveStreamNode(string name, EfCoreSaveArgs<TInEf, TIn, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(EfCoreSaveArgs<TInEf, TIn, TOut> args)
        {
            var ret = args.SourceStream.Observable
                .Chunk(args.BatchSize)
                .Map(i => i.Select(j => (Input: j, Entity: args.GetEntity(j))).ToList())
                .Do(i =>
                {
                    var dbContext = args.KeyedConnection == null
                        ? this.ExecutionContext.DependencyResolver.Resolve<DbContext>()
                        : this.ExecutionContext.DependencyResolver.Resolve<DbContext>(args.KeyedConnection);
                    this.ExecutionContext.InvokeInDedicatedThreadAsync(dbContext, () => ProcessBatch(i, dbContext, args.BulkLoadMode)).Wait();
                })
                .FlatMap((i, ct) => PushObservable.FromEnumerable(i, ct))
                .Map(i => args.GetOutput(i.Input, i.Entity));
            return base.CreateUnsortedStream(ret);
        }
        public void ProcessBatch(List<(TIn Input, TInEf Entity)> items, DbContext dbContext, SaveMode bulkLoadMode)
        {
            var entities = items.Select(i => i.Item2).ToArray();
            var pivotKeys = Args.PivotKeys == null ? (Expression<Func<TInEf, object>>[])null : Args.PivotKeys.ToArray();
            switch (bulkLoadMode)
            {
                case SaveMode.EntityFrameworkCore:
                    dbContext.EfSaveAsync(entities, pivotKeys, Args.SourceStream.Observable.CancellationToken, Args.DoNotUpdateIfExists, Args.InsertOnly).Wait();
                    break;
                case SaveMode.SqlServerBulk:
                    dbContext.BulkSave(entities, pivotKeys, Args.SourceStream.Observable.CancellationToken, Args.DoNotUpdateIfExists, Args.InsertOnly);
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
