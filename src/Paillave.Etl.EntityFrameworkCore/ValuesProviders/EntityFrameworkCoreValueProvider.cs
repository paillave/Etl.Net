using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.EntityFrameworkCore.ValuesProviders
{
    public class EntityFrameworkCoreValueProviderArgs<TIn, TContext, TOut>
    {
        public Func<TIn, TContext, IQueryable<TOut>> GetQuery { get; set; }
        public bool NoParallelisation { get; set; } = false;
    }
    public class EntityFrameworkCoreValueProvider<TIn, TContext, TOut> : ValuesProviderBase<TIn, TContext, TOut> where TContext : DbContext
    {
        private EntityFrameworkCoreValueProviderArgs<TIn, TContext, TOut> _args;
        public EntityFrameworkCoreValueProvider(EntityFrameworkCoreValueProviderArgs<TIn, TContext, TOut> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        public override IDeferedPushObservable<TOut> PushValues(TContext context, TIn input)
        {
            return new DeferedPushObservable<TOut>(pushValue =>
            {
                WaitOne();
                foreach (var item in _args.GetQuery(input, context).ToList())
                    pushValue(item);
                Release();
            });
        }
    }
}
