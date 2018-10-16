using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Paillave.Etl.Core
{
    public interface IValuesProvider<TValueIn, TValueOut>
    {
        IDeferredPushObservable<TValueOut> PushValues(TValueIn args);
    }
    public interface IValuesProvider<TValueIn, TInToApply, TValueOut>
    {
        IDeferredPushObservable<TValueOut> PushValues(TInToApply resource, TValueIn args);
    }
}
