using System;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.EntityFrameworkCore
{
    public class EfCoreValuesProviderArgs<TIn, TOut>
    {
        public string ConnectionKey { get; set; }
        public Func<DbContextWrapper, TIn, IQueryable<TOut>> GetQuery { get; set; }
        public bool StreamMode { get; set; } = false;
    }
    public class EfCoreSingleValueProviderArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
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
        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            var dbContext = _args.ConnectionKey == null
                    ? context.DependencyResolver.Resolve<DbContext>()
                    : context.DependencyResolver.Resolve<DbContext>(_args.ConnectionKey);
            if (_args.StreamMode)
            {
                context.InvokeInDedicatedThreadAsync(dbContext, () =>
                {
                    var lsts = _args.GetQuery(new DbContextWrapper(dbContext), input).AsQueryable();
                    foreach (var elt in lsts) push(elt);
                }).Wait();
            }
            else
            {
                var lsts = context.InvokeInDedicatedThreadAsync(dbContext, () => _args.GetQuery(new DbContextWrapper(dbContext), input).ToList()).Result;
                lsts.ForEach(push);
            }
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
        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            var dbContext = _args.ConnectionKey == null
                    ? context.DependencyResolver.Resolve<DbContext>()
                    : context.DependencyResolver.Resolve<DbContext>(_args.ConnectionKey);
            var res = context.InvokeInDedicatedThreadAsync(dbContext, () => _args.GetQuery(new DbContextWrapper(dbContext), input).FirstOrDefault()).Result;
            push(res);
        }
    }



    public class EfCoreSelectSingleStreamNode<TIn, TOut> : StreamNodeBase<TOut, ISingleStream<TOut>, EfCoreSingleValueProviderArgs<TIn, TOut>>
    {
        public EfCoreSelectSingleStreamNode(string name, EfCoreSingleValueProviderArgs<TIn, TOut> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISingleStream<TOut> CreateOutputStream(EfCoreSingleValueProviderArgs<TIn, TOut> args)
        {
            var obs = args.Stream.Observable.Map(input =>
            {
                var resolver = args.Stream.SourceNode.ExecutionContext.DependencyResolver;
                var invoker = args.Stream.SourceNode.ExecutionContext;
                var dbContext = args.ConnectionKey == null
                        ? resolver.Resolve<DbContext>()
                        : resolver.Resolve<DbContext>(args.ConnectionKey);
                var res = invoker.InvokeInDedicatedThreadAsync(dbContext, () => args.GetQuery(new DbContextWrapper(dbContext), input).FirstOrDefault()).Result;
                return res;
            });
            return base.CreateSingleStream(obs);
        }
    }

}