using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators;

public class MergeSubject<T> : PushSubject<T>
{
    private readonly object _handleCompleteLock = new();
    private class SubscriptionItem
    {
        public IDisposable Subscription { get; set; }
        public IPushObservable<T> Observable { get; set; }
    }
    private readonly IList<SubscriptionItem> _subscriptions = new List<SubscriptionItem>();
    private readonly CancellationTokenSource _linkedCts;

    public MergeSubject(params IPushObservable<T>[] observables)
        : this(CancellationTokenSource.CreateLinkedTokenSource(observables.Select(i => i.CancellationToken).ToArray()), observables) { }

    private MergeSubject(CancellationTokenSource linkedCts, IPushObservable<T>[] observables) : base(linkedCts.Token)
    {
        _linkedCts = linkedCts;
        foreach (var observable in observables)
            _subscriptions.Add(new SubscriptionItem
            {
                Subscription = observable.Subscribe(this.PushValue, () => this.HandleComplete(observable), this.PushException),
                Observable = observable
            });
    }

    private void HandleComplete(IPushObservable<T> observable)
    {
        bool shouldComplete;
        lock (_handleCompleteLock)
        {
            var toDispose = this._subscriptions.FirstOrDefault(i => i.Observable == observable);
            if (toDispose != null)
            {
                toDispose.Subscription.Dispose();
                this._subscriptions.Remove(toDispose);
            }
            shouldComplete = this._subscriptions.Count == 0;
        }
        // Complete() is called outside the lock to avoid a deadlock with
        // OnCompleted() which needs _handleCompleteLock while inside LockObject.
        if (shouldComplete)
            this.Complete();
    }

    protected override void OnCompleted()
    {
        // Dispose any subscriptions that were not yet cleaned up by HandleComplete
        // (e.g. on cancellation before all sources finished).
        lock (_handleCompleteLock)
        {
            foreach (var item in _subscriptions)
                item.Subscription.Dispose();
            _subscriptions.Clear();
        }
        _linkedCts?.Dispose();
    }

    public override void Dispose()
    {
        base.Dispose();
        lock (_handleCompleteLock)
        {
            foreach (var subscription in _subscriptions)
                subscription.Subscription.Dispose();
        }
        _linkedCts?.Dispose();
    }
}
public static partial class ObservableExtensions
{
    public static IPushObservable<T> Merge<T>(this IPushObservable<T> observable, params IPushObservable<T>[] observables)
    {
        return new MergeSubject<T>(new[] { observable }.Union(observables).ToArray());
    }
}
