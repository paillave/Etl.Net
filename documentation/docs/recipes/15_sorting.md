---
sidebar_position: 15
---

# Sorting

For ETL.ET streams can have specific characteristics. These characteristics are sometimes required by some operators. There are some operator to issue streams with there characteristics.

## SortedStream

A `SortedStream` is a stream that issues payloads in a specific order. The stream hosts the sorting criteria and its type for the operators that uses it can operate properly with input payloads.

There are two ways to make a `SortedStream`.

From a `Stream` that issues payloads that are not sorted:

```cs
var sortedStream = stream.Sort("Sort values", i => i.OutputId);
```

To sort on different criterias, and with different direction:

```cs
var sortedStream = stream.Sort("Sort values", i => new { i.OutputId, i.Label });
```

To sort with different directions:

```cs
// Sort Label descending, and then by OutputId
var sortedStream = stream.Sort("Sort values", i => new { i.OutputId, i.Label }, new { Label = -1, OutputId = 2});
```

It can happen that the develop knows that, as a matter of a fact, a `Stream` is actually sorted, for example when values come from a query from a database engine. In this situation, the point is just to check if the stream is actually sorted:

```cs
var sortedStream = stream.EnsureSorted("Ensure sorted", i => i.OutputId);
```

`EnsureSorted` operator work in the same way that `Sort`.

:::warning

If a payload doesn't obey the sorting criteria, the `EnsureSorted` operator will issue an exception, causing the process to fail.

:::

## KeyedStream

A `KeyedStream` is a specific `SortedStream`. It has the same characteristics than a `SortedStream` except that there are no duplicates on the sorting key.

To make a `KeyedStream` out of a stream that is not sorted, first sort it, then ensure it is keyed:

```cs
var keyedStream1 = stream
    .Sort("Sort values1", i => i.OutputId)
    .EnsureKeyed("Ensure values are keyed1", i => i.OutputId);

var keyedStream2 = stream
    .Sort("Sort values2", i => new { i.OutputId, i.Label })
    .EnsureKeyed("Ensure values are keyed2", i => new { i.OutputId, i.Label });

var keyedStream3 = stream
    .Sort("Sort values3", i => new { i.OutputId, i.Label }, new { Label = -1, OutputId = 2})
    .EnsureKeyed("Ensure values are keyed3", i => new { i.OutputId, i.Label }, new { Label = -1, OutputId = 2});
```

Like for `SortedStream` the develop may know that a regular stream is, as a matter of a fact keyed. Then just use `EnsureKeyed` for this purpose.

:::warning

Like `EnsureSorted` operator, `EnsureKeyed` will issue an exception, causing the process to fail, if the stream is not actually keyed.

:::

## SingleStream

A `SingleStream` is a specific `KeyedStream` that has the payload it self a the key criteria. What characterizes it is that not more and not less than one event will be emitted in this stream.

`EnsureSingle` is the way to get a `SingleStream` and checks if the streams is actually a one event stream.

```cs
var singleStream = stream.EnsureSingle("Ensure only one event is emitted");
```

:::warning

`EnsureSingle` will issue an exception, causing the process to fail, if the input stream completes without having emitted a payload previously, or if a payload is emitted whereas another one was issued before.

:::
