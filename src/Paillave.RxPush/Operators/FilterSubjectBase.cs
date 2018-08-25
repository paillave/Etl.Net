using System;
using Paillave.RxPush.Core;

namespace Paillave.RxPush.Operators
{
    public abstract class FilterSubjectBase<T> : PushSubject<T>
    {
        private IDisposable _subscription;

        protected abstract bool AcceptsValue(T value);

        public FilterSubjectBase(IPushObservable<T> observable)
        {
            this._subscription = observable.Subscribe(i =>
            {
                try
                {
                    if (AcceptsValue(i))
                        this.PushValue(i);
                }
                catch (Exception ex)
                {
                    this.PushException(ex);
                }
            }, this.Complete, this.PushException);
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }
}
