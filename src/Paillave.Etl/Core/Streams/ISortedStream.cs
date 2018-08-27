using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.Streams
{
    public interface ISortedStream<T> : IStream<T>, ISortedStream
    {
        IReadOnlyCollection<SortCriteria<T>> SortCriterias { get; }
    }
    public interface ISortedStream : IStream
    {

    }
}