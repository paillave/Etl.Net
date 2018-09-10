using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.EntityFrameworkCore.ValuesProviders
{
    public class EntityFrameworkCoreValueProviderArgs<TIn, TContext, TOut> where TContext : DbContext
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
        protected override void PushValues(TContext resource, TIn input, Action<TOut> pushValue)
        {
            using (base.OpenProcess())
                foreach (var item in _args.GetQuery(input, resource).ToList())
                    pushValue(item);
        }
    }
}
