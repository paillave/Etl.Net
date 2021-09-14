using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;

namespace Paillave.Etl.EntityFrameworkCore
{
    public class EfCoreValuesProviderArgs<TIn, TOut>
    {
        public string ConnectionKey { get; set; }
        public Func<DbContextWrapper, TIn, IQueryable<TOut>> GetQuery { get; set; }
    }
    public class EfCoreSingleValueProviderArgs<TIn, TOut>
    {
        public string ConnectionKey { get; set; }
        public Func<DbContextWrapper, TIn, IQueryable<TOut>> GetQuery { get; set; }
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
            var dbContext = _args.ConnectionKey == null
                    ? resolver.Resolve<DbContext>()
                    : resolver.Resolve<DbContext>(_args.ConnectionKey);
            var lsts = invoker.InvokeInDedicatedThreadAsync(dbContext, () => _args.GetQuery(new DbContextWrapper(dbContext), input).ToList()).Result;
            lsts.ForEach(push);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIn">What enters in the stream</typeparam>
    /// <typeparam name="TOut">What leaves the stream</typeparam>
    public class EfCoreSingleValueProvider<TIn, TOut> : ValuesProviderBase<TIn, TOut>
    {
        private readonly EfCoreSingleValueProviderArgs<TIn, TOut> _args;
        public EfCoreSingleValueProvider(EfCoreSingleValueProviderArgs<TIn, TOut> args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var dbContext = _args.ConnectionKey == null
                    ? resolver.Resolve<DbContext>()
                    : resolver.Resolve<DbContext>(_args.ConnectionKey);
            var res = invoker.InvokeInDedicatedThreadAsync(dbContext, () => _args.GetQuery(new DbContextWrapper(dbContext), input).FirstOrDefault()).Result;
            push(res);
        }
    }
}