---
sidebar_position: 8
---

# Get many data

Getting many data means that for each row entering in an operator, several ones are issued on its output.
Whatever the way to issue many rows always involve a factory method that is called for each occurrence. Every time, this method issues values for the operator.

## Values provider

A value provider is a class that implements the `IValuesProvider` interface  has a method that issues values to the `CrossApply` operator.

```cs title="IValuesProvider.cs"
public interface IValuesProvider<TValueIn, TValueOut>
{
    string TypeName { get; }
    void PushValues(TValueIn input, Action<TValueOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker);
    ProcessImpact PerformanceImpact { get; }
    ProcessImpact MemoryFootPrint { get; }
}
```

:::info

As with the method `PushValues`, nearly 100% of the work is done by the value provider, this one must mention to the `CrossApply` operator how heavy it is; this is the point of properties `PerformanceImpact` and `MemoryFootPrint`.

:::

### Make a value provider

```cs
public class DemoValueProvider : IValuesProvider<string, string>
{
    public string TypeName => "Range of values provider";
    public ProcessImpact PerformanceImpact => ProcessImpact.Light;
    public ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    public void PushValues(string input, Action<string> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
    {
        for (int i = 1; i <= 100; i++)
            push($"{input}{i}");
    }
}
```

### Use a value provider

```cs
contextStream
    .CrossApply<string, string>("create values from factory", new DemoValueProvider())
    .Do("print file name to console", i => Console.WriteLine(i));
```

## Shortcuts to make value providers

`EnumerableValuesProvider` creates a value provider issuing a sequence of payloads from an enumeration depending on the input payload:

```cs
contextStream.CrossApply("create values from enumeration", EnumerableValuesProvider.Create<string, string>(ctx => Enumerable.Range(1, 100).Select(i => $"{ctx}{i}")));
```

To reach the very same this way is shorter:

```cs
contextStream
    .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100).Select(i => $"{ctx}{i}"))
    .Do("print file name to console", i => Console.WriteLine(i));
```

`SimpleValuesProvider` creates a value provider that issues payloads using the given factory:

```cs
contextStream.CrossApply("create values from factory", SimpleValuesProvider.Create<string, string>((ctx, dependencyResolver, cancellationToken, push) =>
    {
        for (int i = 1; i <= 100; i++)
            push($"{ctx}{i}");
    }));
```

As well, an even shorter way exists for this:

```cs
contextStream
    .CrossApply<string, string>("create values from factory", (ctx, dependencyResolver, cancellationToken, push) =>
    {
        for (int i = 1; i <= 100; i++)
            push($"{ctx}{i}");
    })
    .Do("print file name to console", i => Console.WriteLine(i));
```

## Details about `PushValues`

| Parameter | Description |
| - | - |
| `TValueIn input` | The current payload for which several other payloads will be issued by the `CrossApply` operator |
| `Action<TValueOut> push` | The method to call for the `CrossApply` operator to issue a payload |
| `CancellationToken cancellationToken` | The cancellation token that permits to know if the process must be stopped |
| `IDependencyResolver resolver` | This is the access to the dependency injection provided by the runtime |
| `IInvoker invoker` | Use this invoker to make calls of sql server for example. As they cannot be done concurrently, the invoker permits to sequence avery call within the same thread |
