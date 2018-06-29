using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.System
{
    public interface ISortedStream<T> : IStream<T>
    {
        IReadOnlyCollection<SortCriteria<T>> SortCriterias { get; }
    }
}