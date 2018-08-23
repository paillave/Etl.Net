using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.Streams
{
    public interface ISortedStream<T> : IStream<T>, ISortedStream
    {
        IReadOnlyCollection<ISortCriteria<T>> SortCriterias { get; }
    }
    public interface ISortedStream : IStream
    {

    }
}