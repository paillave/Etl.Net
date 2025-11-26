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
        public Func<DbContext, TIn, IQueryable<TOut>> GetQuery { get; set; }
        public bool StreamMode { get; set; } = false;
    }
    public class EfCoreSingleValueProviderArgs<TIn, TOut>
    {
        public IStream<TIn> Stream { get; set; }
        public string ConnectionKey { get; set; }
        public Func<DbContext, TIn, IQueryable<TOut>> GetQuery { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIn">What enters in the stream</typeparam>
    /// <typeparam name="TOut">What leaves the stream</typeparam>
    public class EfCoreValuesProvider<TIn, TOut>(EfCoreValuesProviderArgs<TIn, TOut> args) : ValuesProviderBase<TIn, TOut>
    {
        private readonly EfCoreValuesProviderArgs<TIn, TOut> _args = args;

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            using var ctx = context.Services.GetDbContext(_args.ConnectionKey);

            if (_args.StreamMode)
            {
                var lsts = _args.GetQuery(ctx, input).AsQueryable();
                foreach (var elt in lsts) push(elt);
            }
            else
            {
                var lsts = _args.GetQuery(ctx, input).ToList();
                lsts.ForEach(push);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIn">What enters in the stream</typeparam>
    /// <typeparam name="TOut">What leaves the stream</typeparam>
    public class EfCoreSingleValueProvider<TIn, TOut>(EfCoreSingleValueProviderArgs<TIn, TOut> args) : ValuesProviderBase<TIn, TOut>
    {
        private readonly EfCoreSingleValueProviderArgs<TIn, TOut> _args = args;

        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            using var ctx = context.Services.GetDbContext(_args.ConnectionKey);
            var res = _args.GetQuery(ctx, input).FirstOrDefault();
            push(res);
        }
    }



    public class EfCoreSelectSingleStreamNode<TIn, TOut>(string name, EfCoreSingleValueProviderArgs<TIn, TOut> args)
        : StreamNodeBase<TOut, ISingleStream<TOut>, EfCoreSingleValueProviderArgs<TIn, TOut>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override ISingleStream<TOut> CreateOutputStream(EfCoreSingleValueProviderArgs<TIn, TOut> args)
        {
            var obs = args.Stream.Observable.Map(input =>
            {
                using var ctx = args.Stream.SourceNode.ExecutionContext.Services.GetDbContext(args.ConnectionKey);
                var invoker = args.Stream.SourceNode.ExecutionContext;
                var res = args.GetQuery(ctx, input).FirstOrDefault();
                return res;
            });
            return base.CreateSingleStream(obs);
        }
    }

}