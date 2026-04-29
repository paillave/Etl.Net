using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Paillave.Etl.Reactive.Core;

#if ETL_DEBUG
public static class TmpClass
{
    public static void Log(string line)
    {
        using (var fs = new FileStream("/home/stephane/Desktop/log.txt", FileMode.Append))
        using (var sr = new StreamWriter(fs))
        {
            sr.WriteLine(line);
            fs.Flush();
            sr.Flush();
        }
    }
}
public static class TypeEx
{
    public static string GetFullName(this Type type)
    {
        if (type.IsGenericType)
        {
            return $"{type.Name.Split("`")[0]}<{string.Join(",", type.GetGenericArguments().Select(i => i.GetFullName()))}>";
        }
        else
        {
            return $"{type.Name}";
        }
    }
}
#endif
public class PushSubject<T> : IPushSubject<T>
{
    public CancellationToken CancellationToken { get; }
    private readonly Guid _id = Guid.NewGuid();
    private CancellationTokenRegistration _ctRegistration;
    public PushSubject(CancellationToken cancellationToken)
    {
#if ETL_DEBUG
        var msg = $"{_id}\t<{this.GetType().GetFullName()}>\tCreated";
        Console.WriteLine(msg);
        TmpClass.Log(msg);
#endif
        this.CancellationToken = cancellationToken;
        // Only register when the token can actually be cancelled. Otherwise
        // CancellationToken.Register pins this subject (and its operator graph)
        // to the token's callback list for the lifetime of the token, which
        // for long-lived / external tokens leaks the whole pipeline.
        if (cancellationToken.CanBeCanceled)
            _ctRegistration = cancellationToken.Register(this.Complete);
    }

    private bool _isComplete = false;
    protected object LockObject = new();
    protected ConcurrentList<ISubscription<T>> Subscriptions { get; } = new ConcurrentList<ISubscription<T>>();

    public virtual IDisposable Subscribe(ISubscription<T> subscription)
    {
        lock (LockObject)
        {
            if (this._isComplete && subscription.OnComplete != null) subscription.OnComplete();
            this.Subscriptions.Add(subscription);
            return new Unsubscriber(this, subscription);
        }
    }
    protected bool IsComplete
    {
        get
        {
            lock (LockObject)
                return _isComplete;
        }
    }
    public void  Complete()
    {
        lock (LockObject)
        {
            if (this._isComplete) return;
#if ETL_DEBUG
            var msg = $"{_id}\t<{this.GetType().GetFullName()}>\tCompleted";
            Console.WriteLine(msg);
            TmpClass.Log(msg);
#endif
            this._isComplete = true;
            foreach (var item in this.Subscriptions.ToList())
                if (item.OnComplete != null)
                    item.OnComplete();
            OnCompleted();
        }
        // Release the registration so the subject and its captured graph can
        // be collected even when the CancellationToken outlives them.
        // We use Unregister() (not Dispose()) to avoid a deadlock when this
        // Complete() is itself running as the cancellation callback on
        // another thread: Dispose() would wait for that callback while the
        // callback is waiting for our LockObject.
        _ctRegistration.Unregister();
        _ctRegistration = default;
    }

    /// <summary>
    /// Hook invoked exactly once when this subject completes. Operators
    /// override this to release upstream subscriptions on early termination
    /// (Take, First, TakeUntil, ...) instead of waiting for an explicit
    /// Dispose() call that often never comes.
    /// </summary>
    protected virtual void OnCompleted() { }

    public virtual void Dispose()
    {
        this.Complete();
    }

    public virtual void PushException(Exception exception)
    {
        lock (LockObject)
        {
            if (!this._isComplete)
                foreach (var item in this.Subscriptions.ToList())
                    if (item.OnPushException != null)
                        item.OnPushException(exception);
        }
    }

    protected void TryPushValue(Func<T> getValue)
    {
        try
        {
            var v = getValue();
            PushValue(v);
        }
        catch (Exception ex)
        {
            PushException(ex);
        }
    }
    public void PushValue(T value)
    {
        if (CancellationToken.IsCancellationRequested)
        {
            return;
        }
        lock (LockObject)
        {
            if (!this._isComplete)
                foreach (var item in this.Subscriptions.ToList())
                {
                    try
                    {
                        if (item.OnPushValue != null)
                            item.OnPushValue(value);
                    }
                    catch (Exception ex)
                    {
                        if (item.OnPushException != null)
                            item.OnPushException(ex);
                    }
                }
        }
    }

    public IDisposable Subscribe(Action<T> onPush)
    {
        return Subscribe(new Subscription<T>(onPush));
    }

    public IDisposable Subscribe(Action<T> onPush, Action onComplete)
    {
        return Subscribe(new Subscription<T>(onPush, onComplete));
    }

    public IDisposable Subscribe(Action<T> onPush, Action onComplete, Action<Exception> onException)
    {
        return Subscribe(new Subscription<T>(onPush, onComplete, onException));
    }
    private class Unsubscriber(PushSubject<T> observableBase, ISubscription<T> subscription) : IDisposable
    {
        // var objSync=new Object();
        private readonly PushSubject<T> _observableBase = observableBase;
        private readonly ISubscription<T> _subscription = subscription;

        public void Dispose()
        {
            this._observableBase.Subscriptions.Remove(this._subscription);
        }
    }
    protected class ConcurrentList<U>
    {
        private readonly List<U> lst = new();
        private readonly object syncObject = new();
        public void Remove(U elt)
        {
            lock (syncObject)
            {
                lst.Remove(elt);
            }
        }
        public void Add(U elt)
        {
            lock (syncObject)
            {
                lst.Add(elt);
            }
        }
        public List<U> ToList()
        {
            lock (syncObject)
            {
                return lst.ToList();
            }
        }
    }
}
