using System;

namespace Paillave.RxPush.Core
{
    public interface ISubscription<in T>
    {
        Action OnComplete { get; }
        Action<Exception> OnPushException { get; }
        Action<T> OnPushValue { get; }
    }
}