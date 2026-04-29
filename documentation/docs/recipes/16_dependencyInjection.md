---
sidebar_position: 16
---

# Use injected services

ETL.NET pipelines can call **any service** (logger, repository, HTTP
client, mailer, mock, …) registered in a Microsoft.Extensions
`IServiceProvider`. The runner forwards that provider to every operator
overload that asks for an `IServiceProvider`.

> All snippets are exercised by
> `src/Paillave.Etl.Tests/DocExamples/DependencyInjectionExamplesTests.cs`.

## Wiring the container

Build a service provider as you would for any .NET application, then
hand it to the runner via `ExecutionOptions<TConfig>.Services`.

```cs
var services = new ServiceCollection()
    .AddSingleton<INotifier, InMemoryNotifier>()
    .AddSingleton<IPriceLookup>(new StaticPriceLookup(prices))
    .BuildServiceProvider();

await StreamProcessRunner.CreateAndExecuteAsync(
    config,
    root => /* job */,
    new ExecutionOptions<TConfig> { Services = services });
```

The provider is also exposed to **traces** through
`ExecutionOptions.TraceServices` if you want a separate container for
the trace pipeline.

## Two operator families accept an `IServiceProvider`

| Operator | Signature | Use |
| --- | --- | --- |
| `Do` | `Action<TIn, IServiceProvider>` | Side-effect per row (audit, notify, persist) |
| `SelectResolved` | `Func<TIn, IServiceProvider, TOut>` | Project / enrich a row using injected services |
| `DoCorrelated` / `SelectCorrelated` | `(TIn, IServiceProvider)` overloads | Same, while preserving correlation tokens |

Other operators (`CrossApply`, `Where`, `Select`, …) **do not** receive
the provider directly — capture the dependency you need with a
`SelectResolved` step beforehand, or call into a service from the
preceding `Do` / `SelectResolved` node.

## Example — call a service per row with `Do`

> Test: `Do_WithInjectedService_CallsServiceForEachRow`

```cs {6-10}
var services = new ServiceCollection()
    .AddSingleton<INotifier>(notifier)
    .BuildServiceProvider();

root.CrossApply("seed", _ => new[] { "alpha", "beta", "gamma" })
    .Do("notify", (row, sp) =>
    {
        var n = sp.GetRequiredService<INotifier>();
        n.Notify($"processed:{row}");
    });
```

This pattern is well-suited for **side effects** that don't change the
stream's shape: audit logs, telemetry, sending events, calling APIs.

## Example — enrich rows with `SelectResolved`

> Test: `SelectResolved_EnrichesRowsFromInjectedService`

```cs {7-15}
var services = new ServiceCollection()
    .AddSingleton<IPriceLookup>(new StaticPriceLookup(prices))
    .BuildServiceProvider();

root.CrossApply("seed", _ => orderLines)
    .SelectResolved("price", (row, sp) =>
    {
        var lookup = sp.GetRequiredService<IPriceLookup>();
        return new PricedOrderLine
        {
            Sku       = row.Sku,
            Quantity  = row.Quantity,
            UnitPrice = lookup.GetPrice(row.Sku),
        };
    })
    .Do("collect", _ => /* ... */);
```

Use `SelectResolved` when you need to **transform** the row using data
the container can produce (lookups, configuration, current user,
remote services).

## Example — combine multiple services

> Test: `Pipeline_CanResolveSeveralServices`

```cs
.SelectResolved("price", (row, sp) => new PricedOrderLine
{
    Sku       = row.Sku,
    Quantity  = row.Quantity,
    UnitPrice = sp.GetRequiredService<IPriceLookup>().GetPrice(row.Sku),
})
.Do("audit", (row, sp) =>
{
    sp.GetRequiredService<INotifier>()
      .Notify($"{row.Sku} x{row.Quantity} = {row.Total}");
});
```

You can resolve as many services as you need from a single lambda — the
provider is the same one that was registered on `ExecutionOptions`.

## Tips and gotchas

- **Always register your services before calling `ExecuteAsync`.**
  The runner does not allow late registrations.
- **Use `GetRequiredService<T>()`** rather than `GetService<T>()` to
  fail fast if a binding is missing.
- **Lifetimes**: the provider you pass is used as-is. Singletons live
  for the duration of your application, scoped services need a manual
  `IServiceScopeFactory` if you want a fresh scope per row (see
  [`CreateDbContextScope`](./6_ef.md)).
- **Avoid heavy work inside `SelectResolved`** if it can be done once
  upstream — resolve a stateful service and reuse it row-after-row in
  a `Do` block when possible.
- **Database / EF Core**: prefer the dedicated
  [`CreateDbContextScope`](./6_ef.md) helper or the EF operators
  (`EfCoreSelect`, `EfCoreSave`) — they internally pull the
  `DbContext` from the same provider.

## Cheat sheet

| Intent | Operator |
| --- | --- |
| Per-row side effect using a service | `Do(name, (row, sp) => ...)` |
| Projection / enrichment using a service | `SelectResolved(name, (row, sp) => ...)` |
| Same on a correlated stream | `DoCorrelated`, `SelectResolved` (overloads) |
| Inject services | `new ExecutionOptions<TConfig> { Services = sp }` |
