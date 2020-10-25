using Paillave.Etl.Core;
using System;
using System.Threading;

namespace Paillave.Etl.ValuesProviders
{
    public class ValueProviderWrapper<TIn, TInnerIn, TInnerOut, TOut> : IValuesProvider<TIn, TOut>
    {
        private IValuesProvider<TInnerIn, TInnerOut> _innerValueProvider;
        private readonly Func<TIn, TInnerIn> _getInnerIn;
        private readonly Func<TInnerOut, TIn, TOut> _getOut;

        public ValueProviderWrapper(IValuesProvider<TInnerIn, TInnerOut> innerValueProvider, Func<TIn, TInnerIn> getInnerIn, Func<TInnerOut, TIn, TOut> getOut)
        {
            _innerValueProvider = innerValueProvider;
            this._getInnerIn = getInnerIn;
            this._getOut = getOut;
        }

        public string TypeName => _innerValueProvider.TypeName;

        public ProcessImpact PerformanceImpact => _innerValueProvider.PerformanceImpact;

        public ProcessImpact MemoryFootPrint => _innerValueProvider.MemoryFootPrint;

        public void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver)
            => _innerValueProvider.PushValues(_getInnerIn(input), i => push(_getOut(i, input)), cancellationToken, resolver);
    }
}
