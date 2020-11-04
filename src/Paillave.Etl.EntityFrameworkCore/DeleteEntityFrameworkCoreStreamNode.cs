using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using Microsoft.EntityFrameworkCore;
using Paillave.EntityFrameworkCoreExtension.Core;

namespace Paillave.Etl.EntityFrameworkCore
{
    public class DeleteEntityFrameworkCoreArgsBuilder<TIn>
    {
        internal IStream<TIn> InputStream { get; }
        internal DeleteEntityFrameworkCoreArgsBuilder(IStream<TIn> inputStream)
            => (InputStream) = (inputStream);
        public DeleteEntityFrameworkCoreArgsBuilder<TIn, TEntity> Set<TEntity>(string keyedConnection = null) where TEntity : class
            => new DeleteEntityFrameworkCoreArgsBuilder<TIn, TEntity>(this, keyedConnection);
    }
    public class DeleteEntityFrameworkCoreArgsBuilder<TIn, TEntity> where TEntity : class
    {
        internal DeleteEntityFrameworkCoreArgsBuilder<TIn> Parent { get; }
        internal string KeyedConnection { get; private set; }
        internal DeleteEntityFrameworkCoreArgsBuilder(DeleteEntityFrameworkCoreArgsBuilder<TIn> parent, string keyedConnection)
            => (Parent, KeyedConnection) = (parent, keyedConnection);
        internal Expression<Func<TIn, TEntity, bool>> WhereStatement { get; private set; } = null;
        public DeleteEntityFrameworkCoreArgsBuilder<TIn, TEntity> Where(Expression<Func<TIn, TEntity, bool>> match)
        {
            this.WhereStatement = match;
            return this;
        }
        internal DeleteEntityFrameworkCoreArgs<TIn, TIn, TEntity> BuildArgs()
            => new DeleteEntityFrameworkCoreArgs<TIn, TIn, TEntity>
            {
                InputStream = this.Parent.InputStream,
                GetValue = i => i,
                Match = this.WhereStatement,
                KeyedConnection = this.KeyedConnection
            };
    }

    public class DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn>
    {
        internal IStream<Correlated<TIn>> InputStream { get; }
        internal DeleteEntityFrameworkCoreCorrelatedArgsBuilder(IStream<Correlated<TIn>> inputStream)
            => (InputStream) = (inputStream);
        public DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn, TEntity> Set<TEntity>(string keyedConnection = null) where TEntity : class
            => new DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn, TEntity>(this, keyedConnection);
    }
    public class DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn, TEntity> where TEntity : class
    {
        private DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn> Parent { get; }
        internal string KeyedConnection { get; private set; }
        public DeleteEntityFrameworkCoreCorrelatedArgsBuilder(DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn> parent, string keyedConnection)
            => (Parent, KeyedConnection) = (parent, keyedConnection);
        internal Expression<Func<TIn, TEntity, bool>> WhereStatement { get; private set; } = null;
        public DeleteEntityFrameworkCoreCorrelatedArgsBuilder<TIn, TEntity> Where(Expression<Func<TIn, TEntity, bool>> match)
        {
            this.WhereStatement = match;
            return this;
        }
        internal DeleteEntityFrameworkCoreArgs<Correlated<TIn>, TIn, TEntity> BuildArgs()
            => new DeleteEntityFrameworkCoreArgs<Correlated<TIn>, TIn, TEntity>
            {
                InputStream = this.Parent.InputStream,
                GetValue = i => i.Row,
                Match = this.WhereStatement,
                KeyedConnection = this.KeyedConnection
            };
    }


    // public class DeleteWhereEntityFrameworkCoreArgsBuilder<TIn, TEntity> where TEntity : class
    // {
    //     private readonly IStream<TIn> _inputStream;
    //     public DeleteWhereEntityFrameworkCoreArgsBuilder(IStream<TIn> inputStream)
    //         => (_inputStream) = (inputStream);
    //     public DeleteEntityFrameworkCoreArgs<TIn, TIn, TEntity> Where(Expression<Func<TIn, TEntity, bool>> match)
    //         => new DeleteEntityFrameworkCoreArgs<TIn, TIn, TEntity>
    //         {

    //         };
    // }
    public class DeleteEntityFrameworkCoreArgs<TIn, TValue, TEntity>
        where TEntity : class
    {
        public IStream<TIn> InputStream { get; set; }
        public Expression<Func<TValue, TEntity, bool>> Match { get; set; }
        public Func<TIn, TValue> GetValue { get; set; }
        internal string KeyedConnection { get; set; } = null;
    }
    public class DeleteEntityFrameworkCoreStreamNode<TIn, TValue, TEntity> : StreamNodeBase<TIn, IStream<TIn>, DeleteEntityFrameworkCoreArgs<TIn, TValue, TEntity>>
        where TEntity : class
    {
        public DeleteEntityFrameworkCoreStreamNode(string name, DeleteEntityFrameworkCoreArgs<TIn, TValue, TEntity> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override IStream<TIn> CreateOutputStream(DeleteEntityFrameworkCoreArgs<TIn, TValue, TEntity> args)
        {
            var matchingS = args.InputStream.Observable
                .Map(i =>
                {
                    var ctx = args.KeyedConnection == null
                        ? this.ExecutionContext.DependencyResolver.Resolve<DbContext>()
                        : this.ExecutionContext.DependencyResolver.Resolve<DbContext>(args.KeyedConnection);
                    TValue val = args.GetValue(i);
                    this.ExecutionContext.InvokeInDedicatedThread(ctx, () =>
                    {
                        ctx.Set<TEntity>().DeleteWhereAsync(args.Match.ApplyPartialLeft<TValue, TEntity, bool>(val), args.InputStream.Observable.CancellationToken).Wait();
                    });
                    return i;
                });
            return base.CreateUnsortedStream(matchingS);
        }
    }
}