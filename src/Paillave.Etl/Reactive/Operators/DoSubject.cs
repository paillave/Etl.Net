using System;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class DoSubject<T> : PushSubject<T>
    {
        private IDisposable _subscription;
        private object _syncValue = new object();

        public DoSubject(IPushObservable<T> observable, Action<T> action) : base(observable.CancellationToken)
        {
            this._subscription = observable.Subscribe(i =>
            {
                if (CancellationToken.IsCancellationRequested)
                {
                    return;
                }
                lock (_syncValue)
                {
                    try
                    {
                        action(i);
                        this.PushValue(i);
                    }
                    catch (Exception ex)
                    {
                        this.PushException(ex);
                    }
                }
            }, this.Complete, this.PushException);
        }

        public override void Dispose()
        {
            lock (_syncValue)
            {
                base.Dispose();
                _subscription.Dispose();
            }
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<T> Do<T>(this IPushObservable<T> observable, Action<T> action) => new DoSubject<T>(observable, action);
    }
}
