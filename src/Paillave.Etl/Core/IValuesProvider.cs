using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Paillave.Etl.Core
{
    public interface IValuesProvider<TIn, TOut>
    {
        //void SetWaitHandle(WaitHandle waitHandle);
        IDeferedPushObservable<TOut> PushValues(TIn args);
    }
    public interface IValuesProvider<TIn, TRes, TOut>
    {
        //void SetWaitHandle(WaitHandle waitHandle);
        IDeferedPushObservable<TOut> PushValues(TRes resource, TIn args);
    }
}
