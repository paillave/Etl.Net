﻿using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core.Streams
{
    public interface ISortedStream<T> : IStream<T>
    {
        IReadOnlyCollection<ISortCriteria<T>> SortCriterias { get; }
    }
}