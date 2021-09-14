using System;
using System.Collections.Generic;
using System.Threading;

namespace Paillave.Etl.Core
{
    public class EnumerableValuesProvider
    {
        public static EnumerableValuesProvider<TIn, TOut> Create<TIn, TOut>(Func<TIn, IEnumerable<TOut>> getValues) => new EnumerableValuesProvider<TIn, TOut>(getValues);
        public static EnumerableValuesProvider<TIn1, TIn2, TOut> Create<TIn1, TIn2, TOut>(Func<TIn1, TIn2, IEnumerable<TOut>> getValues) => new EnumerableValuesProvider<TIn1, TIn2, TOut>(getValues);
    }
    public class EnumerableValuesProvider<TIn, TOut> : ValuesProviderBase<TIn, TOut>
    {
        private Func<TIn, IEnumerable<TOut>> _getValues;

        public EnumerableValuesProvider(Func<TIn, IEnumerable<TOut>> getValues)
        {
            _getValues = getValues;
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            foreach (var value in _getValues(input))
            {
                if (cancellationToken.IsCancellationRequested) break;
                push(value);
            }
        }
    }
    public class EnumerableValuesProvider<TIn1, TIn2, TOut> : ValuesProviderBase<TIn1, TIn2, TOut>
    {
        private Func<TIn1, TIn2, IEnumerable<TOut>> _getValues;

        public EnumerableValuesProvider(Func<TIn1, TIn2, IEnumerable<TOut>> getValues)
        {
            _getValues = getValues;
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public override void PushValues(TIn1 input1, TIn2 input2, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            foreach (var value in _getValues(input1, input2))
            {
                if (cancellationToken.IsCancellationRequested) break;
                push(value);
            }
        }
    }
}