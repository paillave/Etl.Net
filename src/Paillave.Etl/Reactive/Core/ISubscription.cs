using System;

namespace Paillave.Etl.Reactive.Core;

public interface ISubscription<in T>
{
    Action OnComplete { get; }
    Action<Exception> OnPushException { get; }
    Action<T> OnPushValue { get; }
}