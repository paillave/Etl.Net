using System;
using System.Threading;

namespace Paillave.Etl.Core
{
    public interface IValuesProvider<TValueIn, TValueOut>
    {
        string TypeName { get; }
        void PushValues(TValueIn input, Action<TValueOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker);
        ProcessImpact PerformanceImpact { get; }
        ProcessImpact MemoryFootPrint { get; }
    }
    public abstract class ValuesProviderBase<TValueIn, TValueOut> : IValuesProvider<TValueIn, TValueOut>
    {
        public virtual string TypeName => this.GetType().Name;
        public abstract void PushValues(TValueIn input, Action<TValueOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker);
        public abstract ProcessImpact PerformanceImpact { get; }
        public abstract ProcessImpact MemoryFootPrint { get; }
    }
    public interface IValuesProvider<TValueIn, TInToApply, TValueOut>
    {
        string TypeName { get; }
        void PushValues(TValueIn input, TInToApply toApply, Action<TValueOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker);
        ProcessImpact PerformanceImpact { get; }
        ProcessImpact MemoryFootPrint { get; }
    }
    public abstract class ValuesProviderBase<TValueIn, TInToApply, TValueOut> : IValuesProvider<TValueIn, TInToApply, TValueOut>
    {
        public virtual string TypeName => this.GetType().Name;
        public abstract ProcessImpact PerformanceImpact { get; }
        public abstract ProcessImpact MemoryFootPrint { get; }
        public abstract void PushValues(TValueIn input, TInToApply toApply, Action<TValueOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker);
    }
}