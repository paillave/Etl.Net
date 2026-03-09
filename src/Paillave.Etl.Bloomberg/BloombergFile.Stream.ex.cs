using System;
using Paillave.Etl.Core;
using System.Linq.Expressions;
using Paillave.Etl.Core.Mapping;

namespace Paillave.Etl.Bloomberg;

public static class BloombergFileEx
{
    public static IStream<BloombergResult<TOut>> CrossApplyBloombergFile<TOut>(this IStream<IFileValue> stream, string name, Expression<Func<IFieldMapper, TOut>> expression, bool noParallelisation = false)
        => stream.CrossApply(name, BloombergValuesProvider.Create<TOut>(expression), noParallelisation);
}
