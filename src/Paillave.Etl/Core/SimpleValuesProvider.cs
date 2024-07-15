using System;
using System.Collections.Generic;
using System.Threading;

namespace Paillave.Etl.Core
{
    public class SimpleValuesProvider
    {
        public static SimpleValuesProvider<TIn, TOut> Create<TIn, TOut>(Action<TIn, CancellationToken, Action<TOut>> pushValues) => new SimpleValuesProvider<TIn, TOut>(pushValues);
        public static SimpleValuesProvider<TIn, TOut> Create<TIn, TOut>(Action<TIn, IExecutionContext, CancellationToken, Action<TOut>> pushValues) => new SimpleValuesProvider<TIn, TOut>(pushValues);
        // public static SimpleValuesProvider<TIn1, TIn2, TOut> Create<TIn1, TIn2, TOut>(Func<TIn1, TIn2, IEnumerable<TOut>> getValues) => new SimpleValuesProvider<TIn1, TIn2, TOut>(getValues);
    }
    public class SimpleValuesProvider<TIn, TOut> : ValuesProviderBase<TIn, TOut>
    {
        private Action<TIn, IExecutionContext, CancellationToken, Action<TOut>> _pushValuesWithResolver = null;
        private Action<TIn, CancellationToken, Action<TOut>> _pushValues = null;

        public SimpleValuesProvider(Action<TIn, CancellationToken, Action<TOut>> pushValues) =>
            _pushValues = pushValues;
        public SimpleValuesProvider(Action<TIn, IExecutionContext, CancellationToken, Action<TOut>> pushValues) => 
            _pushValuesWithResolver = pushValues;

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public override void PushValues(TIn input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            if (_pushValuesWithResolver != null)
                _pushValuesWithResolver(input, context, cancellationToken, push);
            else if (_pushValues != null)
                _pushValues(input, cancellationToken, push);
        }
    }
    // public class SimpleValuesProvider<TIn1, TIn2, TOut> : ValuesProviderBase<TIn1, TIn2, TOut>
    // {
    //     private Func<TIn1, TIn2, IEnumerable<TOut>> _getValues;

    //     public SimpleValuesProvider(Func<TIn1, TIn2, IEnumerable<TOut>> getValues)
    //     {
    //         _getValues = getValues;
    //     }

    //     public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

    //     public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

    //     public override void PushValues(TIn1 input1, TIn2 input2, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
    //     {
    //         foreach (var value in _getValues(input1, input2))
    //         {
    //             if (cancellationToken.IsCancellationRequested) break;
    //             push(value);
    //         }
    //     }
    // }
}