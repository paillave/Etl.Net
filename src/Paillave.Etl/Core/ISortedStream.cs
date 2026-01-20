using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core;

public interface ISortedStream<T, TKey> : IStream<T>, ISortedStream
{
    SortDefinition<T, TKey> SortDefinition { get; }
}
public interface ISortedStream : IStream
{

}
