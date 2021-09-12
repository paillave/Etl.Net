---
sidebar_position: 12
---

# Change/Create in stream data

## Select

`Select` is the regular way to create data within a stream. What is done in a `Select` should ideally be pure.

The `Select` operator works in the same way than the `Select` of linq, (except that the name of the operation must be transmitted as a first parameter).

```cs {3-8}
contextStream
    .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100))
    .Select("Create a value", i => new
    {
        Id = i,
        OutputId = i % 3,
        Label = $"Label{i}",
    })
    .Do("print file name to console", i => Console.WriteLine($"{i.Id}-{i.Label}"));
```

If it is necessary to keep a context during the execution for an operator, a class that implements `ISelectProcessor` must be done.

Here is an example of a generic select processor.

```cs
class ValueProcessorWithContext<TIn, TOut, TCtx> : ISelectProcessor<TIn, TOut>
{
    private readonly TCtx _context;
    private readonly Func<TIn, TCtx, TOut> _process;
    public ValueProcessorWithContext(TCtx context, Func<TIn, TCtx, TOut> process) 
        => (_context, _process) = (context, process);
    public TOut ProcessRow(TIn value) 
        => _process(value, _context);
}
```

Here is the way this select processor can be used:

```cs
contextStream
    .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100))
    .Select("Create a value", new ValueProcessorWithContext<int, string, TempoContext>(
        new TempoContext(),
        (int v, TempoContext ctx) =>
        {
            if (v == 12)
                ctx.Value1++;
            if (v == 5)
                ctx.Value2 = $"5 value already passed";
            return $"{ctx.Value1}-{v} {ctx.Value2}";
        }))
    .Do("print file name to console", Console.WriteLine);
```

## Fix

`Select` is supposed to work with a pure mechanism. In the situation of simple amendments, this can be very verbose, and compels to create new values.

`Fix` permits to issue an updated payload in a clear and readable way, even if the payload is anonymous.

```cs
contextStream
    .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100))
    .Select("Create a value", i => new
    {
        Id = i,
        OutputId = i % 3,
        Label = (i % 3 == 2) ? null : $"Label{i}",
    })
    .Fix("fix value", o => o
        .FixProperty(i => i.OutputId).AlwaysWith(i => i.OutputId * 10)
        .FixProperty(i => i.Label).IfNullWith(i => $"New Label {i.Id}"))
    .Do("print file name to console", i => Console.WriteLine($"{i.Id}-{i.Label}"));
```
