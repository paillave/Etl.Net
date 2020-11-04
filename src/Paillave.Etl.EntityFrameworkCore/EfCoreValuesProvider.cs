using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Paillave.EntityFrameworkCoreExtension.Core;
using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.EntityFrameworkCore
{
    public static class EfCoreValuesProviderArgsEx
    {
        public static EfCoreValuesProviderArgsBuilderOrderByDescending<TIn, TEntity, TKey> OrderByDescending<TIn, TEntity, TKey>(this EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, TKey>> orderClause)
            where TEntity : class
            => new EfCoreValuesProviderArgsBuilderOrderByDescending<TIn, TEntity, TKey>(from, orderClause);
        public static EfCoreValuesProviderArgsBuilderOrderBy<TIn, TEntity, TKey> OrderBy<TIn, TEntity, TKey>(this EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, TKey>> orderClause)
            where TEntity : class
            => new EfCoreValuesProviderArgsBuilderOrderBy<TIn, TEntity, TKey>(from, orderClause);
        public static EfCoreValuesProviderArgsBuilderTop<TIn, TEntity> Top<TIn, TEntity, TKey>(this EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, int top)
            where TEntity : class
            => new EfCoreValuesProviderArgsBuilderTop<TIn, TEntity>(from, top);
        public static EfCoreValuesProviderArgsBuilderWhere<TIn, TEntity> Where<TIn, TEntity>(this EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, bool>> queryCriteria)
            where TEntity : class
            => new EfCoreValuesProviderArgsBuilderWhere<TIn, TEntity>(from, queryCriteria);
        public static EfCoreValuesProviderArgsBuilderWhereWithValue<TIn, TEntity> Where<TIn, TEntity>(this EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, TIn, bool>> queryCriteria)
            where TEntity : class
            => new EfCoreValuesProviderArgsBuilderWhereWithValue<TIn, TEntity>(from, queryCriteria);
        public static EfCoreValuesProviderArgsBuilderInclude<TIn, TEntity, TProp> Include<TIn, TEntity, TProp>(this EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, TProp>> include)
            where TEntity : class
            where TProp : class
            => new EfCoreValuesProviderArgsBuilderInclude<TIn, TEntity, TProp>(from, include);
        public static EfCoreValuesProviderArgsBuilderThenInclude<TIn, TEntity, TFromProp, TToProp> ThenInclude<TIn, TEntity, TFromProp, TToProp>(this EfCoreValuesProviderArgsBuilderInclude<TIn, TEntity, TFromProp> from, Expression<Func<TFromProp, TToProp>> thenInclude)
            where TEntity : class
            where TFromProp : class
            where TToProp : class
            => new EfCoreValuesProviderArgsBuilderThenInclude<TIn, TEntity, TFromProp, TToProp>(from, thenInclude);
        public static EfCoreValuesProviderArgsBuilderPostProcess<TIn, TEntity, TOut> Select<TIn, TEntity, TOut>(this EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Func<TEntity, TIn, TOut> postProcess)
            where TEntity : class
            => new EfCoreValuesProviderArgsBuilderPostProcess<TIn, TEntity, TOut>(from, postProcess);
    }
    public abstract class EfCoreValuesProviderArgsBase<TIn, TOut>
    {
        internal EfCoreValuesProviderArgsBase() { }
        internal abstract List<TOut> GetResult(DbContext dbContext, TIn value);
        internal abstract DbContext GetDbContext(IDependencyResolver resolver);
    }
    public abstract class EfCoreValuesProviderArgsBase<TIn, TEntity, TOut> : EfCoreValuesProviderArgsBase<TIn, TOut> where TEntity : class
    {
        internal EfCoreValuesProviderArgsBase() { }
        internal abstract IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value);
    }
    public abstract class EfCoreValuesProviderArgsIncluderBase<TIn, TEntity, TProp, TOut> : EfCoreValuesProviderArgsBase<TIn, TEntity, TOut> where TEntity : class where TProp : class
    {
        internal EfCoreValuesProviderArgsIncluderBase() { }
        internal abstract IIncludableQueryable<TEntity, TProp> GetIncludableQueryable(DbContext dbContext, TIn value);
    }
    public class EfCoreValuesProviderArgsBuilder<TIn>
    {
        internal EfCoreValuesProviderArgsBuilder() { }
        public EfCoreValuesProviderArgsBuilderSet<TIn, TEntity> Set<TEntity>(string keyedConnection = null) where TEntity : class => new EfCoreValuesProviderArgsBuilderSet<TIn, TEntity>(keyedConnection);
    }
    public class EfCoreValuesProviderArgsBuilderSet<TIn, TEntity> : EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> where TEntity : class
    {
        internal readonly string _keyedConnection = null;
        internal EfCoreValuesProviderArgsBuilderSet(string keyedConnection) => (_keyedConnection) = (keyedConnection);
        internal override DbContext GetDbContext(IDependencyResolver resolver)
            => _keyedConnection == null
                ? resolver.Resolve<DbContext>()
                : resolver.Resolve<DbContext>(_keyedConnection);
        internal override IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value) => dbContext.Set<TEntity>();
        internal override List<TEntity> GetResult(DbContext dbContext, TIn value) => GetQueryable(dbContext, value).ToList();
    }
    public class EfCoreValuesProviderArgsBuilderTop<TIn, TEntity> : EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> where TEntity : class
    {
        private readonly EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> _from;
        private readonly int _top;
        internal EfCoreValuesProviderArgsBuilderTop(EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, int top) => (_from, _top) = (from, top);
        internal override IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value) => _from.GetQueryable(dbContext, value).Take(_top);
        internal override List<TEntity> GetResult(DbContext dbContext, TIn value) => GetQueryable(dbContext, value).ToList();
        internal override DbContext GetDbContext(IDependencyResolver resolver) => _from.GetDbContext(resolver);
    }
    public class EfCoreValuesProviderArgsBuilderWhere<TIn, TEntity> : EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> where TEntity : class
    {
        private readonly EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> _from;
        private readonly Expression<Func<TEntity, bool>> _queryCriteria;
        internal EfCoreValuesProviderArgsBuilderWhere(EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, bool>> queryCriteria) => (_from, _queryCriteria) = (from, queryCriteria);
        internal override IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value) => _from.GetQueryable(dbContext, value).Where(_queryCriteria);
        internal override List<TEntity> GetResult(DbContext dbContext, TIn value) => GetQueryable(dbContext, value).ToList();
        internal override DbContext GetDbContext(IDependencyResolver resolver) => _from.GetDbContext(resolver);
    }
    public class EfCoreValuesProviderArgsBuilderOrderBy<TIn, TEntity, TKey> : EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> where TEntity : class
    {
        private readonly EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> _from;
        private readonly Expression<Func<TEntity, TKey>> _orderBy;
        internal EfCoreValuesProviderArgsBuilderOrderBy(EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, TKey>> orderBy) => (_from, _orderBy) = (from, orderBy);
        internal override IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value) => _from.GetQueryable(dbContext, value).OrderBy(_orderBy);
        internal override List<TEntity> GetResult(DbContext dbContext, TIn value) => GetQueryable(dbContext, value).ToList();
        internal override DbContext GetDbContext(IDependencyResolver resolver) => _from.GetDbContext(resolver);
    }
    public class EfCoreValuesProviderArgsBuilderOrderByDescending<TIn, TEntity, TKey> : EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> where TEntity : class
    {
        private readonly EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> _from;
        private readonly Expression<Func<TEntity, TKey>> _orderByDescending;
        internal EfCoreValuesProviderArgsBuilderOrderByDescending(EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, TKey>> orderByDescending) => (_from, _orderByDescending) = (from, orderByDescending);
        internal override IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value) => _from.GetQueryable(dbContext, value).OrderByDescending(_orderByDescending);
        internal override List<TEntity> GetResult(DbContext dbContext, TIn value) => GetQueryable(dbContext, value).ToList();
        internal override DbContext GetDbContext(IDependencyResolver resolver) => _from.GetDbContext(resolver);
    }
    public class EfCoreValuesProviderArgsBuilderWhereWithValue<TIn, TEntity> : EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> where TEntity : class
    {
        private readonly EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> _from;
        private readonly Expression<Func<TEntity, TIn, bool>> _queryCriteria;
        internal EfCoreValuesProviderArgsBuilderWhereWithValue(EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, TIn, bool>> queryCriteria) => (_from, _queryCriteria) = (from, queryCriteria);
        internal override IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value) => _from.GetQueryable(dbContext, value).Where(_queryCriteria.ApplyPartialRight(value));
        internal override List<TEntity> GetResult(DbContext dbContext, TIn value) => GetQueryable(dbContext, value).ToList();
        internal override DbContext GetDbContext(IDependencyResolver resolver) => _from.GetDbContext(resolver);
    }
    public class EfCoreValuesProviderArgsBuilderInclude<TIn, TEntity, TProp> : EfCoreValuesProviderArgsIncluderBase<TIn, TEntity, TProp, TEntity> where TEntity : class where TProp : class
    {
        private readonly EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> _from;
        private readonly Expression<Func<TEntity, TProp>> _include;
        internal EfCoreValuesProviderArgsBuilderInclude(EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Expression<Func<TEntity, TProp>> include) => (_from, _include) = (from, include);
        internal override IIncludableQueryable<TEntity, TProp> GetIncludableQueryable(DbContext dbContext, TIn value) => _from.GetQueryable(dbContext, value).Include(_include);
        internal override IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value) => GetIncludableQueryable(dbContext, value);
        internal override List<TEntity> GetResult(DbContext dbContext, TIn value) => GetQueryable(dbContext, value).ToList();
        internal override DbContext GetDbContext(IDependencyResolver resolver) => _from.GetDbContext(resolver);
    }
    public class EfCoreValuesProviderArgsBuilderThenInclude<TIn, TEntity, TFromProp, TToProp> : EfCoreValuesProviderArgsIncluderBase<TIn, TEntity, TToProp, TEntity> where TEntity : class where TFromProp : class where TToProp : class
    {
        private readonly EfCoreValuesProviderArgsIncluderBase<TIn, TEntity, TFromProp, TEntity> _from;
        private readonly Expression<Func<TFromProp, TToProp>> _thenInclude;
        internal EfCoreValuesProviderArgsBuilderThenInclude(EfCoreValuesProviderArgsIncluderBase<TIn, TEntity, TFromProp, TEntity> from, Expression<Func<TFromProp, TToProp>> thenInclude) => (_from, _thenInclude) = (from, thenInclude);
        internal override IIncludableQueryable<TEntity, TToProp> GetIncludableQueryable(DbContext dbContext, TIn value) => _from.GetIncludableQueryable(dbContext, value).ThenInclude(_thenInclude);
        internal override IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value) => GetIncludableQueryable(dbContext, value);
        internal override List<TEntity> GetResult(DbContext dbContext, TIn value) => GetQueryable(dbContext, value).ToList();
        internal override DbContext GetDbContext(IDependencyResolver resolver) => _from.GetDbContext(resolver);
    }
    public class EfCoreValuesProviderArgsBuilderPostProcess<TIn, TEntity, TOut> : EfCoreValuesProviderArgsBase<TIn, TEntity, TOut> where TEntity : class
    {
        private readonly EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> _from;
        private readonly Func<TEntity, TIn, TOut> _postProcess;
        internal EfCoreValuesProviderArgsBuilderPostProcess(EfCoreValuesProviderArgsBase<TIn, TEntity, TEntity> from, Func<TEntity, TIn, TOut> postProcess) => (_from, _postProcess) = (from, postProcess);
        internal override IQueryable<TEntity> GetQueryable(DbContext dbContext, TIn value) => _from.GetQueryable(dbContext, value);
        internal override List<TOut> GetResult(DbContext dbContext, TIn value) => GetQueryable(dbContext, value).ToList().Select(i => _postProcess(i, value)).ToList();
        internal override DbContext GetDbContext(IDependencyResolver resolver) => _from.GetDbContext(resolver);
    }
    public class EfCoreValuesProviderArgs<TIn, TOut>
    {
        public IStream<TIn> InputStream { get; set; }
        public EfCoreValuesProviderArgsBase<TIn, TOut> Arguments { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIn">What enters in the stream</typeparam>
    /// <typeparam name="TOut">What leaves the stream</typeparam>
    public class EfCoreValuesProvider<TIn, TOut> : ValuesProviderBase<TIn, TOut>
    {
        private readonly EfCoreValuesProviderArgs<TIn, TOut> _args;
        public EfCoreValuesProvider(EfCoreValuesProviderArgs<TIn, TOut> args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var dbContext = this._args.Arguments.GetDbContext(resolver);
            var lsts = invoker.InvokeInDedicatedThread(dbContext, () => _args.Arguments.GetResult(dbContext, input));
            lsts.ForEach(push);
        }
    }
}