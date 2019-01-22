using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class DeferredPushObservable<T> : PushObservableBase<T>, IDeferredPushObservable<T>
    {
        private Action<Action<T>> _valuesFactory;
        public DeferredPushObservable(Action<Action<T>> valuesFactory)
        {
            _valuesFactory = valuesFactory;
        }

        private Guid tmp = Guid.NewGuid();
        public void Start()
        {
            Task.Run(() => InternStart());
        }
        private void InternStart()
        {
            lock (LockObject)
            {
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
            }
        }
    }
}
