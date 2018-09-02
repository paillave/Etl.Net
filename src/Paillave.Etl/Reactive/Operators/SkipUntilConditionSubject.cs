using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Operators
{
    public class SkipUntilConditionSubject<TIn> : PushSubject<TIn>
    {
        private object _lockObject = new object();
        private IDisposable _disp1;
        private bool _isTriggered = false;
        private Func<TIn, bool> _condition;
        private bool _included;
        public SkipUntilConditionSubject(IPushObservable<TIn> observable, Func<TIn, bool> condition, bool included = true)
        {
            _condition = condition;
            _included = included;
            _disp1 = observable.Subscribe(HandleOnPush, HandleOnComplete, HandleOnError);
        }

        private void HandleOnError(Exception ex)
        {
            lock (_lockObject)
            {
                if (_isTriggered) PushException(ex);
            }
        }

        private void HandleOnComplete()
        {
            lock (_lockObject)
            {
                Complete();
            }
        }

        private void HandleOnPush(TIn value)
        {
            lock (_lockObject)
            {
                if (_isTriggered) PushValue(value);
                else
                {
                    try
                    {
                        bool conditionReached = _condition(value);
                        if (conditionReached)
                        {
                            _isTriggered = true;
                            if (_included) PushValue(value);
                        }
                    }
                    catch (Exception ex)
                    {
                        PushException(ex);
                    }
                }
            }
        }

        public override void Dispose()
        {
            _disp1.Dispose();
            base.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TIn> SkipUntil<TIn>(this IPushObservable<TIn> observable, Func<TIn, bool> condition, bool included = true)
        {
            return new SkipUntilConditionSubject<TIn>(observable, condition, included);
        }
    }
}
