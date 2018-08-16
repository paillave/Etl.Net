using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public interface IValuesProvider<TArgs, TOut>
    {
        void PushValues(TArgs args, Action<TOut> pushValue);
    }
}
