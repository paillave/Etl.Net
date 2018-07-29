using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.System.Streams
{
    public interface IKeyedStream<T> : ISortedStream<T>
    {
    }
}