using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;
using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.EntityFrameworkCore.ValuesProviders
{
    public class EntityFrameworkCoreValueProviderArgs<TIn, TRes, TOut>
    {
        public bool NoParallelisation { get; set; } = false;
        public Func<TIn, TRes, IQueryable<TOut>> GetQuery { get; set; }
    }
    public class EntityFrameworkCoreValueProvider<TIn, TRes, TOut> : ValuesProviderBase<TIn, TRes, TOut> where TRes : DbContext
    {
        private EntityFrameworkCoreValueProviderArgs<TIn, TRes, TOut> _args;
        public EntityFrameworkCoreValueProvider(EntityFrameworkCoreValueProviderArgs<TIn, TRes, TOut> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        public override IDeferedPushObservable<TOut> PushValues(TRes resource, TIn input)
        {
            return new DeferedPushObservable<TOut>(pushValue =>
            {
                WaitOne();
                foreach (var item in _args.GetQuery(input, resource).ToList())
                    pushValue(item);
                Release();
            });
        }
    }
}
