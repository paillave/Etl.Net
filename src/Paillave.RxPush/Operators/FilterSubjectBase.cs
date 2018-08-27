using System;
using Paillave.RxPush.Core;

namespace Paillave.RxPush.Operators
{
    public abstract class FilterSubjectBase<T> : PushSubject<T>
    {
        private IDisposable _subscription;

        protected abstract bool AcceptsValue(T value);
        private object _syncValue = new object();

        public FilterSubjectBase(IPushObservable<T> observable)
        {
            this._subscription = observable.Subscribe(HandlePushValue, this.Complete, this.PushException);
        }

        private void HandlePushValue(T value)
        {
            lock (_syncValue)
            {
                try
                {
                    if (AcceptsValue(value))
                        this.PushValue(value);
                }
                catch (Exception ex)
                {
                    this.PushException(ex);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }
}
