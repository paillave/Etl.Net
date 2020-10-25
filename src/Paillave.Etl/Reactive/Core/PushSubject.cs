using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Paillave.Etl.Reactive.Core
{
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
        private Guid _id = Guid.NewGuid();
        public PushSubject(CancellationToken cancellationToken)
        {
#if ETL_DEBUG
            var msg = $"{_id}\t<{this.GetType().GetFullName()}>\tCreated";
            Console.WriteLine(msg);
            TmpClass.Log(msg);
#endif
            this.CancellationToken = cancellationToken;
            cancellationToken.Register(this.Complete);
        }

        private bool _isComplete = false;
        protected object LockObject = new object();
        protected ConcurrentList<ISubscription<T>> Subscriptions { get; } = new ConcurrentList<ISubscription<T>>();

        public virtual IDisposable Subscribe(ISubscription<T> subscription)
        {
            lock (LockObject)
            {
                if (this._isComplete) subscription.OnComplete();
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
        public void Complete()
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
                    item.OnComplete();
            }
        }

        public virtual void Dispose()
        {
            this.Complete();
        }

        public void PushException(Exception exception)
        {
            lock (LockObject)
            {
                if (!this._isComplete)
                    foreach (var item in this.Subscriptions.ToList())
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
                            item.OnPushValue(value);
                        }
                        catch (Exception ex)
                        {
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
        private class Unsubscriber : IDisposable
        {
            // var objSync=new Object();
            private PushSubject<T> _observableBase;
            private ISubscription<T> _subscription;

            public Unsubscriber(PushSubject<T> observableBase, ISubscription<T> subscription)
            {
                this._observableBase = observableBase;
                this._subscription = subscription;
            }

            public void Dispose()
            {
                this._observableBase.Subscriptions.Remove(this._subscription);
            }
        }
        protected class ConcurrentList<U>
        {
            private List<U> lst = new List<U>();
            private object syncObject = new Object();
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
}
