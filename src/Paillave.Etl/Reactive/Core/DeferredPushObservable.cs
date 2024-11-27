using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class DeferredPushObservable<T>(Action<Action<T>, CancellationToken> valuesFactory, CancellationToken cancellationToken) : PushSubject<T>(cancellationToken), IDeferredPushObservable<T>
    {
        private Action<Action<T>, CancellationToken> _valuesFactory = valuesFactory;
        private Guid tmp = Guid.NewGuid();
        public void Start() => Task.Run(() => InternStart());
        private void InternStart()
        {
            lock (LockObject)
            {
                try
                {
                    _valuesFactory(PushValue, base.CancellationToken);
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
