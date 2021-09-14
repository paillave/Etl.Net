using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core
{
    public interface IKeyedStream<T, TKey> : ISortedStream<T, TKey>, IKeyedStream
    {
    }
    public interface IKeyedStream : ISortedStream
    {

    }
}
