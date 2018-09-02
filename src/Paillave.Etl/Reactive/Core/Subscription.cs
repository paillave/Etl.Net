using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Reactive.Core
{
    public class Subscription<T> : ISubscription<T>
    {
        public Subscription(Action<T> onPush)
        {
            this.OnPushValue = onPush;
            this.OnComplete = () => { };
            this.OnPushException = (e) => { };
        }
        public Subscription(Action<T> onPush, Action onComplete)
        {
            this.OnPushValue = onPush;
            this.OnComplete = onComplete;
            this.OnPushException = (e) => { };
        }
        public Subscription(Action<T> onPush, Action onComplete, Action<Exception> onException)
        {
            this.OnPushValue = onPush;
            this.OnComplete = onComplete;
            this.OnPushException = onException;
        }
        public Action<T> OnPushValue { get; }
        public Action OnComplete { get; }
        public Action<Exception> OnPushException { get; }
    }
}
