using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.Streams
{
    public interface ISortedStream<T, TKey> : IStream<T>, ISortedStream
    {
        SortDefinition<T, TKey> SortDefinition { get; }
    }
    public interface ISortedStream : IStream
    {

    }
}
