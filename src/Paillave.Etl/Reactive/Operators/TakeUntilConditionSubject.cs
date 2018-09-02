using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Operators
{
    public class TakeUntilConditionSubject<TIn> : PushSubject<TIn>
    {
        private object _lockObject = new object();
        private IDisposable _disp1;
        private Func<TIn, bool> _condition;
        private bool _included;
        public TakeUntilConditionSubject(IPushObservable<TIn> observable, Func<TIn, bool> condition, bool included = false)
        {
            _condition = condition;
            _included = included;
            _disp1 = observable.Subscribe(HandleOnPush, Complete, PushException);
        }

        private void HandleOnPush(TIn obj)
        {
            lock (_lockObject)
            {
                if (!this.IsComplete)
                {
                    try
                    {
                        bool conditionReached = _condition(obj);
                        if (conditionReached)
                        {
                            if (_included) PushValue(obj);
                            Complete();
                        }
                        else
                            PushValue(obj);
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
        public static IPushObservable<TIn> TakeUntil<TIn>(this IPushObservable<TIn> observable, Func<TIn, bool> condition, bool included = false)
        {
            return new TakeUntilConditionSubject<TIn>(observable, condition, included);
        }
    }
}
