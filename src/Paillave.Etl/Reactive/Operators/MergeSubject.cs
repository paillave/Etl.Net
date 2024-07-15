using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class MergeSubject<T> : PushSubject<T>
    {
        private object _handleCompleteLock = new object();
        private class SubscriptionItem
        {
            public IDisposable Subscription { get; set; }
            public IPushObservable<T> Observable { get; set; }
        }
        private IList<SubscriptionItem> _subscriptions = new List<SubscriptionItem>();

        public MergeSubject(params IPushObservable<T>[] observables) : base(CancellationTokenSource.CreateLinkedTokenSource(observables.Select(i => i.CancellationToken).ToArray()).Token)
        {
            foreach (var observable in observables)
                _subscriptions.Add(new SubscriptionItem
                {
                    Subscription = observable.Subscribe(this.PushValue, () => this.HandleComplete(observable), this.PushException),
                    Observable = observable
                });
        }

        private void HandleComplete(IPushObservable<T> observable)
        {
            lock (_handleCompleteLock)
            {
                var toDispose = this._subscriptions.FirstOrDefault(i => i.Observable == observable);
                if (toDispose != null)
                {
                    toDispose.Subscription.Dispose();
                    this._subscriptions.Remove(toDispose);
                }
                if (this._subscriptions.Count == 0)
                    this.Complete();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var subscription in _subscriptions)
                subscription.Subscription.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<T> Merge<T>(this IPushObservable<T> observable, params IPushObservable<T>[] observables) => new MergeSubject<T>(new[] { observable }.Union(observables).ToArray());
    }
}
