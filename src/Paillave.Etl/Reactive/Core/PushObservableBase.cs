//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
////https://joshclose.github.io/CsvHelper/
//namespace Paillave.Etl.Reactive.Core
//{
//    public abstract class PushObservableBase<T> : IPushSubject<T>
//    {
//        private class SubscriptionManager
//        {
//            private interface IMethodCaller
//            {
//                void OnComplete();
//                void OnPushValue(T value);
//                void OnPushException(Exception exception);
//            }
//            private class MethodCaller0 : IMethodCaller
//            {
//                public MethodCaller0() { }
//                public void OnComplete() { }
//                public void OnPushException(Exception exception) { }
//                public void OnPushValue(T value) { }
//            }
//            private class MethodCaller1 : IMethodCaller
//            {
//                private ISubscription<T> _subscription1;
//                public MethodCaller1(ISubscription<T> subscription1)
//                {
//                    _subscription1 = subscription1;
//                }
//                public void OnComplete()
//                {
//                    _subscription1.OnComplete();
//                }

//                public void OnPushException(Exception exception)
//                {
//                    _subscription1.OnPushException(exception);
//                }

//                public void OnPushValue(T value)
//                {
//                    _subscription1.OnPushValue(value);
//                }
//            }
//            private class MethodCaller2 : IMethodCaller
//            {
//                private ISubscription<T> _subscription2;
//                private ISubscription<T> _subscription1;
//                public MethodCaller2(ISubscription<T> subscription1, ISubscription<T> subscription2)
//                {
//                    _subscription1 = subscription1;
//                    _subscription2 = subscription2;
//                }
//                public void OnComplete()
//                {
//                    _subscription1.OnComplete();
//                    _subscription2.OnComplete();
//                }

//                public void OnPushException(Exception exception)
//                {
//                    _subscription1.OnPushException(exception);
//                    _subscription2.OnPushException(exception);
//                }

//                public void OnPushValue(T value)
//                {
//                    _subscription1.OnPushValue(value);
//                    _subscription2.OnPushValue(value);
//                }
//            }
//            private class MethodCaller3 : IMethodCaller
//            {
//                private ISubscription<T> _subscription1;
//                private ISubscription<T> _subscription2;
//                private ISubscription<T> _subscription3;
//                public MethodCaller3(ISubscription<T> subscription1, ISubscription<T> subscription2, ISubscription<T> subscription3)
//                {
//                    _subscription1 = subscription1;
//                    _subscription2 = subscription2;
//                    _subscription3 = subscription3;
//                }
//                public void OnComplete()
//                {
//                    _subscription1.OnComplete();
//                    _subscription2.OnComplete();
//                    _subscription3.OnComplete();
//                }

//                public void OnPushException(Exception exception)
//                {
//                    _subscription1.OnPushException(exception);
//                    _subscription2.OnPushException(exception);
//                    _subscription3.OnPushException(exception);
//                }

//                public void OnPushValue(T value)
//                {
//                    _subscription1.OnPushValue(value);
//                    _subscription2.OnPushValue(value);
//                    _subscription3.OnPushValue(value);
//                }
//            }
//            private class MethodCaller4 : IMethodCaller
//            {
//                private ISubscription<T> _subscription1;
//                private ISubscription<T> _subscription2;
//                private ISubscription<T> _subscription3;
//                private ISubscription<T> _subscription4;
//                public MethodCaller4(ISubscription<T> subscription1, ISubscription<T> subscription2, ISubscription<T> subscription3, ISubscription<T> subscription4)
//                {
//                    _subscription1 = subscription1;
//                    _subscription2 = subscription2;
//                    _subscription3 = subscription3;
//                    _subscription4 = subscription4;
//                }
//                public void OnComplete()
//                {
//                    _subscription1.OnComplete();
//                    _subscription2.OnComplete();
//                    _subscription3.OnComplete();
//                    _subscription4.OnComplete();
//                }

//                public void OnPushException(Exception exception)
//                {
//                    _subscription1.OnPushException(exception);
//                    _subscription2.OnPushException(exception);
//                    _subscription3.OnPushException(exception);
//                    _subscription4.OnPushException(exception);
//                }

//                public void OnPushValue(T value)
//                {
//                    _subscription1.OnPushValue(value);
//                    _subscription2.OnPushValue(value);
//                    _subscription3.OnPushValue(value);
//                    _subscription4.OnPushValue(value);
//                }
//            }
//            private class MethodCaller5 : IMethodCaller
//            {
//                private ISubscription<T> _subscription1;
//                private ISubscription<T> _subscription2;
//                private ISubscription<T> _subscription3;
//                private ISubscription<T> _subscription4;
//                private ISubscription<T> _subscription5;
//                public MethodCaller5(ISubscription<T> subscription1, ISubscription<T> subscription2, ISubscription<T> subscription3, ISubscription<T> subscription4, ISubscription<T> subscription5)
//                {
//                    _subscription1 = subscription1;
//                    _subscription2 = subscription2;
//                    _subscription3 = subscription3;
//                    _subscription4 = subscription4;
//                    _subscription5 = subscription5;
//                }
//                public void OnComplete()
//                {
//                    _subscription1.OnComplete();
//                    _subscription2.OnComplete();
//                    _subscription3.OnComplete();
//                    _subscription4.OnComplete();
//                    _subscription5.OnComplete();
//                }

//                public void OnPushException(Exception exception)
//                {
//                    _subscription1.OnPushException(exception);
//                    _subscription2.OnPushException(exception);
//                    _subscription3.OnPushException(exception);
//                    _subscription4.OnPushException(exception);
//                    _subscription5.OnPushException(exception);
//                }

//                public void OnPushValue(T value)
//                {
//                    _subscription1.OnPushValue(value);
//                    _subscription2.OnPushValue(value);
//                    _subscription3.OnPushValue(value);
//                    _subscription4.OnPushValue(value);
//                    _subscription5.OnPushValue(value);
//                }
//            }
//            private class MethodCaller6 : IMethodCaller
//            {
//                private ISubscription<T> _subscription1;
//                private ISubscription<T> _subscription2;
//                private ISubscription<T> _subscription3;
//                private ISubscription<T> _subscription4;
//                private ISubscription<T> _subscription5;
//                private ISubscription<T> _subscription6;
//                public MethodCaller6(ISubscription<T> subscription1, ISubscription<T> subscription2, ISubscription<T> subscription3, ISubscription<T> subscription4, ISubscription<T> subscription5, ISubscription<T> subscription6)
//                {
//                    _subscription1 = subscription1;
//                    _subscription2 = subscription2;
//                    _subscription3 = subscription3;
//                    _subscription4 = subscription4;
//                    _subscription5 = subscription5;
//                    _subscription6 = subscription6;
//                }
//                public void OnComplete()
//                {
//                    _subscription1.OnComplete();
//                    _subscription2.OnComplete();
//                    _subscription3.OnComplete();
//                    _subscription4.OnComplete();
//                    _subscription5.OnComplete();
//                    _subscription6.OnComplete();
//                }

//                public void OnPushException(Exception exception)
//                {
//                    _subscription1.OnPushException(exception);
//                    _subscription2.OnPushException(exception);
//                    _subscription3.OnPushException(exception);
//                    _subscription4.OnPushException(exception);
//                    _subscription5.OnPushException(exception);
//                    _subscription6.OnPushException(exception);
//                }

//                public void OnPushValue(T value)
//                {
//                    _subscription1.OnPushValue(value);
//                    _subscription2.OnPushValue(value);
//                    _subscription3.OnPushValue(value);
//                    _subscription4.OnPushValue(value);
//                    _subscription5.OnPushValue(value);
//                    _subscription6.OnPushValue(value);
//                }
//            }
//            private class MethodCaller7 : IMethodCaller
//            {
//                private ISubscription<T> _subscription1;
//                private ISubscription<T> _subscription2;
//                private ISubscription<T> _subscription3;
//                private ISubscription<T> _subscription4;
//                private ISubscription<T> _subscription5;
//                private ISubscription<T> _subscription6;
//                private ISubscription<T> _subscription7;
//                public MethodCaller7(ISubscription<T> subscription1, ISubscription<T> subscription2, ISubscription<T> subscription3, ISubscription<T> subscription4, ISubscription<T> subscription5, ISubscription<T> subscription6, ISubscription<T> subscription7)
//                {
//                    _subscription1 = subscription1;
//                    _subscription2 = subscription2;
//                    _subscription3 = subscription3;
//                    _subscription4 = subscription4;
//                    _subscription5 = subscription5;
//                    _subscription6 = subscription6;
//                    _subscription7 = subscription7;
//                }
//                public void OnComplete()
//                {
//                    _subscription1.OnComplete();
//                    _subscription2.OnComplete();
//                    _subscription3.OnComplete();
//                    _subscription4.OnComplete();
//                    _subscription5.OnComplete();
//                    _subscription6.OnComplete();
//                    _subscription7.OnComplete();
//                }

//                public void OnPushException(Exception exception)
//                {
//                    _subscription1.OnPushException(exception);
//                    _subscription2.OnPushException(exception);
//                    _subscription3.OnPushException(exception);
//                    _subscription4.OnPushException(exception);
//                    _subscription5.OnPushException(exception);
//                    _subscription6.OnPushException(exception);
//                    _subscription7.OnPushException(exception);
//                }

//                public void OnPushValue(T value)
//                {
//                    _subscription1.OnPushValue(value);
//                    _subscription2.OnPushValue(value);
//                    _subscription3.OnPushValue(value);
//                    _subscription4.OnPushValue(value);
//                    _subscription5.OnPushValue(value);
//                    _subscription6.OnPushValue(value);
//                    _subscription7.OnPushValue(value);
//                }
//            }
//            private class MethodCallerList : IMethodCaller
//            {
//                private IEnumerable<ISubscription<T>> _subscriptions;
//                public MethodCallerList(IEnumerable<ISubscription<T>> subscriptions)
//                {
//                    _subscriptions = subscriptions;
//                }
//                public void OnComplete()
//                {
//                    foreach (var item in this._subscriptions.ToList())
//                        item.OnComplete();
//                }

//                public void OnPushException(Exception exception)
//                {
//                    foreach (var item in this._subscriptions.ToList())
//                        item.OnPushException(exception);
//                }

//                public void OnPushValue(T value)
//                {
//                    foreach (var item in this._subscriptions.ToList())
//                        item.OnPushValue(value);
//                }
//            }
//            private IList<ISubscription<T>> _subscriptions = new List<ISubscription<T>>();
//            private IMethodCaller _methodCaller = new MethodCaller0();
//            private void SetMethodCaller()
//            {
//                switch (this._subscriptions.Count)
//                {
//                    case 0:
//                        _methodCaller = new MethodCaller0();
//                        break;
//                    case 1:
//                        _methodCaller = new MethodCaller1(this._subscriptions[0]);
//                        break;
//                    case 2:
//                        _methodCaller = new MethodCaller2(this._subscriptions[0], this._subscriptions[1]);
//                        break;
//                    case 3:
//                        _methodCaller = new MethodCaller3(this._subscriptions[0], this._subscriptions[1], this._subscriptions[2]);
//                        break;
//                    case 4:
//                        _methodCaller = new MethodCaller4(this._subscriptions[0], this._subscriptions[1], this._subscriptions[2], this._subscriptions[3]);
//                        break;
//                    case 5:
//                        _methodCaller = new MethodCaller5(this._subscriptions[0], this._subscriptions[1], this._subscriptions[2], this._subscriptions[3], this._subscriptions[4]);
//                        break;
//                    case 6:
//                        _methodCaller = new MethodCaller6(this._subscriptions[0], this._subscriptions[1], this._subscriptions[2], this._subscriptions[3], this._subscriptions[4], this._subscriptions[5]);
//                        break;
//                    case 7:
//                        _methodCaller = new MethodCaller7(this._subscriptions[0], this._subscriptions[1], this._subscriptions[2], this._subscriptions[3], this._subscriptions[4], this._subscriptions[5], this._subscriptions[6]);
//                        break;
//                    default:
//                        _methodCaller = new MethodCallerList(this._subscriptions);
//                        break;
//                }
//            }
//            public IDisposable Add(ISubscription<T> subscription)
//            {
//                this._subscriptions.Add(subscription);
//                SetMethodCaller();
//                return new Unsubscriber(this, subscription);
//            }
//            public void CallComplete()
//            {
//                _methodCaller.OnComplete();
//            }
//            public void CallPushException(Exception exception)
//            {
//                _methodCaller.OnPushException(exception);
//            }
//            public void CallPushValue(T value)
//            {
//                _methodCaller.OnPushValue(value);
//            }
//            private class Unsubscriber : IDisposable
//            {
//                private SubscriptionManager _subscriptionManager;
//                private ISubscription<T> _subscription;

//                public Unsubscriber(SubscriptionManager subscriptionManager, ISubscription<T> subscription)
//                {
//                    this._subscriptionManager = subscriptionManager;
//                    this._subscription = subscription;
//                }

//                public void Dispose()
//                {
//                    this._subscriptionManager._subscriptions.Remove(this._subscription);
//                    this._subscriptionManager.SetMethodCaller();
//                }
//            }
//        }
//        private bool _isComplete = false;
//        protected object LockObject = new object();
//        private SubscriptionManager _subscriptions { get; } = new SubscriptionManager();
//        //protected IList<ISubscription<T>> Subscriptions { get; } = new List<ISubscription<T>>();

//        public virtual IDisposable Subscribe(ISubscription<T> subscription)
//        {
//            lock (LockObject)
//            {
//                if (this._isComplete) subscription.OnComplete();
//                return this._subscriptions.Add(subscription);
//            }
//        }
//        protected bool IsComplete
//        {
//            get
//            {
//                lock (LockObject)
//                    return _isComplete;
//            }
//        }
//        public void Complete()
//        {
//            lock (LockObject)
//            {
//                if (this._isComplete) return;
//                this._isComplete = true;
//                _subscriptions.CallComplete();
//            }
//        }

//        public virtual void Dispose()
//        {
//            this.Complete();
//        }

//        public void PushException(Exception exception)
//        {
//            lock (LockObject)
//            {
//                if (!this._isComplete)
//                    _subscriptions.CallPushException(exception);
//            }
//        }

//        public void PushValue(T value)
//        {
//            lock (LockObject)
//            {
//                if (!this._isComplete)
//                    _subscriptions.CallPushValue(value);
//            }
//        }

//        public IDisposable Subscribe(Action<T> onPush)
//        {
//            return Subscribe(new Subscription<T>(onPush));
//        }

//        public IDisposable Subscribe(Action<T> onPush, Action onComplete)
//        {
//            return Subscribe(new Subscription<T>(onPush, onComplete));
//        }

//        public IDisposable Subscribe(Action<T> onPush, Action onComplete, Action<Exception> onException)
//        {
//            return Subscribe(new Subscription<T>(onPush, onComplete, onException));
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public abstract class PushObservableBase<T> : IPushSubject<T>
    {
        private bool _isComplete = false;
        protected object LockObject = new object();
        protected IList<ISubscription<T>> Subscriptions { get; } = new List<ISubscription<T>>();

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

        public void PushValue(T value)
        {
            lock (LockObject)
            {
                if (!this._isComplete)
                    foreach (var item in this.Subscriptions.ToList())
                        item.OnPushValue(value);
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
        // public override IDisposable Subscribe(ISubscription<T> subscription)
        // {
        //     lock (lockObject)
        //     {
        //         if (this._isComplete) subscription.OnComplete();
        //         return this.Subscribe(subscription);
        //     }
        // }

        private class Unsubscriber : IDisposable
        {
            private PushObservableBase<T> _observableBase;
            private ISubscription<T> _subscription;

            public Unsubscriber(PushObservableBase<T> observableBase, ISubscription<T> subscription)
            {
                this._observableBase = observableBase;
                this._subscription = subscription;
            }

            public void Dispose()
            {
                this._observableBase.Subscriptions.Remove(this._subscription);
            }
        }
    }
}
