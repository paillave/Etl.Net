using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.RxPush.Core
{
    public class DeferedPushObservable<T> : PushObservableBase<T>, IDeferedPushObservable<T>
    {
        private bool _isComplete = false;
        private Action<Action<T>> _valuesFactory;
        private bool _startOnFirstSubscription;
        private object lockObject = new object();
        public DeferedPushObservable(Action<Action<T>> valuesFactory, bool startOnFirstSubscription = false) //Get rid of the startOnFirstSubscription parameter by replacing it with an EventWaitHandle parameter
        {
            _valuesFactory = valuesFactory;
            _startOnFirstSubscription = startOnFirstSubscription;
        }
        private void Complete()
        {
            lock (lockObject)
            {
                foreach (var item in base.Subscriptions.ToList())
                    item.OnComplete();
                this._isComplete = true;
            }
        }

        public virtual void Dispose()
        {
            this.Complete();
        }

        public void PushException(Exception exception)
        {
            lock (lockObject)
            {
                foreach (var item in base.Subscriptions.ToList())
                    item.OnPushException(exception);
            }
        }

        private void PushValue(T value)
        {
            lock (lockObject)
            {
                foreach (var item in base.Subscriptions.ToList())
                    item.OnPushValue(value);
            }
        }

        public void Start()
        {
            Task.Run(() =>
            {
                //HORRIBLE trick to increase chances that the following subscription chain is build before the process starts to issue the first value
                System.Threading.Thread.Sleep(100);

                //TODO: get rid of the horrible previous trick by using an EventWaitHandle in such a way. The ewt.Set() would be done outside, anywhere
                ////EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.ManualReset);
                ////var tasks = Enumerable.Range(0, 4).Select(_ => Task.Run(() => {
                ////    ewh.WaitOne();
                ////    //do the work here
                ////})).ToArray();
                ////ewh.Set();
                ////Task.WaitAll(tasks);

                try
                {
                    _valuesFactory(PushValue);
                }
                catch (Exception ex)
                {
                    PushException(ex);
                }
                finally
                {
                    Complete();
                }
            });
        }

        public override IDisposable Subscribe(ISubscription<T> subscription)
        {
            lock (lockObject)
            {
                if (this._isComplete) subscription.OnComplete();
                var subs = base.Subscribe(subscription);
                if (_startOnFirstSubscription)
                {
                    _startOnFirstSubscription = false;
                    this.Start();
                }
                return subs;
            }
        }
    }
}
