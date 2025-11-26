using System;
using System.Collections.Generic;

namespace Paillave.Etl.Core;

#region Simple select
public interface ISelectProcessor<TIn, TOut>
{
    TOut ProcessRow(TIn value, IServiceProvider services);
}
public class SimpleSelectProcessor<TIn, TOut>(Func<TIn, IServiceProvider, TOut> selector) : ISelectProcessor<TIn, TOut>
{
    public TOut ProcessRow(TIn value, IServiceProvider services) => selector(value, services);
}
public class SelectWithSequenceProcessor<TIn, TKey, TOut>(Func<TIn, int, IServiceProvider, TOut> selector, Func<TIn, TKey> keySelector) : ISelectProcessor<TIn, TOut>
{
    private readonly Dictionary<TKey, int> _sequences = [];
    public TOut ProcessRow(TIn value, IServiceProvider services)
    {
        var key = keySelector(value);
        _sequences.TryGetValue(key, out int sequence);
        _sequences[key] = ++sequence;
        return selector(value, sequence, services);
    }
}
public class ContextReference<TCtx>(TCtx context)
{
    public TCtx Context { get; set; } = context;
}
public class ContextSelectProcessor<TIn, TOut, TCtx>(Func<TIn, ContextReference<TCtx>, IServiceProvider, TOut> selector, TCtx initialContext) : ISelectProcessor<TIn, TOut>
{
    private readonly ContextReference<TCtx> _contextReference = new(initialContext);
    public TOut ProcessRow(TIn value, IServiceProvider services) => selector(value, _contextReference, services);
}
#endregion

#region Select with index
public interface ISelectWithIndexProcessor<TIn, TOut>
{
    TOut ProcessRow(TIn value, int index, IServiceProvider services);
}
public class SimpleSelectWithIndexProcessor<TIn, TOut>(Func<TIn, int, IServiceProvider, TOut> selector) : ISelectWithIndexProcessor<TIn, TOut>
{
    public TOut ProcessRow(TIn value, int index, IServiceProvider services) => selector(value, index, services);
}
public class ContextSelectWithIndexProcessor<TIn, TOut, TCtx>(Func<TIn, int, ContextReference<TCtx>, IServiceProvider, TOut> selector, TCtx initialContext) : ISelectWithIndexProcessor<TIn, TOut>
{
    private readonly ContextReference<TCtx> _contextReference = new(initialContext);
    public TOut ProcessRow(TIn value, int index, IServiceProvider services) => selector(value, index, _contextReference, services);
}
#endregion
