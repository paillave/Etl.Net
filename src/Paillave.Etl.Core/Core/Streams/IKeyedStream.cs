using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.Streams
{
    public interface IKeyedStream<T> : ISortedStream<T>
    {
    }
}