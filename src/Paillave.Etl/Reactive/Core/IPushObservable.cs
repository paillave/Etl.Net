using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public interface IPushObservable<out T>
    {
        IDisposable Subscribe(Action<T> onPush);
        IDisposable Subscribe(Action<T> onPush, Action onComplete);
        IDisposable Subscribe(Action<T> onPush, Action onComplete, Action<Exception> onException);
        IDisposable Subscribe(ISubscription<T> subscription);
    }
}
