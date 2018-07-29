using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.System.Streams
{
    public interface IStream<T>
    {
        IPushObservable<T> Observable { get; }
        IExecutionContext ExecutionContext { get; }
    }
}