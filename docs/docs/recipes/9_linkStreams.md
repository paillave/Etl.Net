---
sidebar_position: 9
---

# Combine streams

Combining streams is the essence of SQL. It is very important to have efficient stream combiners.

## Select

Although `Select` is an operator dedicated to transform the content of the payload, it can also combine the content of payloads of a stream with the content of a single event stream `ISingleStream`.

```cs
private static void DefineProcess(ISingleStream<string> contextStream)
{
    var res = contextStream
        .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100))
        .Select("apply single stream content", contextStream, (l, r) => $"{l}-{r}");
    // ...
}
```

## Join and LeftJoin

There is no `Join` operator; just a `LeftJoin` exists. Like a regular SQL left join, the right value will be null if nothing matches.
Like in SSIS, the left input stream must be properly sorted depending on the left pivot key, and the stream that is looked up must be keyed on depending on the right pivot key. Both keys must have exactly the same signature (name and types).

```cs {15}
private static void DefineProcess(ISingleStream<string> contextStream)
{
    var sortedStream1 = contextStream
        .CrossApply("create values from enumeration", ctx => Enumerable
            .Range(1, 100)
            .Select(i => new { Id = i, OutputId = i % 10, label = $"Label{i}" }))
        .EnsureSorted("ensure it is sorted on OutputId", i => new { i.OutputId });

    var streamToLookup = contextStream
        .CrossApply("create values from enumeration2", ctx => Enumerable
            .Range(1, 8)
            .Select(i => new { Id = i, label = $"OtherLabel{i}" }))
        .EnsureKeyed("ensure it is keyed on Id", i => new { OutputId = i.Id });

    var res = sortedStream1.LeftJoin("join output values", streamToLookup, (l, r) => new { FromLeft = l, FromRight = r });
    // ...
}
```

:::note

`LeftJoin` is the operator to chose in favor of `Lookup` is streams are sorted because nothing remains in memory during the process.
It goes without saying that internally, if streams were not sorted, the operator would have to make it itself. But it may happen that these input streams are already sorted, and then, by sorting it again, the operator would lose precious time, and moreover would have to store both streams content in memory before issuing the output. To prevent this, the way to deal with sorts must be done by the developer. Either by actually sorting, either by simply ensuring the stream is sorted.
If input streams are not sorted, it will be a more convenient choice to deal with the `Lookup` operator.

:::

## Lookup

`Lookup` has the same purpose than `LeftJoin` and behaves in the same way, but does all the underlying mechanism is done under the hood for non sorted streams.

```cs
private static void DefineProcess(ISingleStream<string> contextStream)
{
    var stream1 = contextStream
        .CrossApply("create values from enumeration", ctx => Enumerable
            .Range(1, 100)
            .Select(i => new { Id = i, OutputId = i % 10, label = $"Label{i}" }));

    var streamToLookup = contextStream
        .CrossApply("create values from enumeration2", ctx => Enumerable
            .Range(1, 8)
            .Select(i => new { Id = i, label = $"OtherLabel{i}" }));

    var res = stream1.Lookup("join output values", streamToLookup, l => l.OutputId, r => r.Id, (l, r) => new { FromLeft = l, FromRight = r });
    // ...
}
```

## Union/Merge

Merging two streams together can be done with the `Union` operator.

```cs
private static void DefineProcess(ISingleStream<string> contextStream)
{
    var stream1 = contextStream
        .CrossApply("create values from enumeration", ctx => Enumerable
            .Range(1, 100)
            .Select(i => new { Id = i, label = $"Label{i}" }));
    var stream2 = contextStream
        .CrossApply("create values from enumeration2", ctx => Enumerable
            .Range(1, 8)
            .Select(i => new { Id = i, label = $"OtherLabel{i}" }));

    var res = stream1.Union("merge with stream 2", stream2);
    // ...
}
```

## UnionAll

```cs
private static void DefineProcess(ISingleStream<string> contextStream)
{
    var stream1 = contextStream
        .CrossApply("create values from enumeration", ctx => Enumerable
            .Range(1, 100)
            .Select(i => new { Id = i, label = $"Label{i}" }));
    var stream2 = contextStream
        .CrossApply("create values from enumeration2", ctx => Enumerable
            .Range(1, 8)
            .Select(i => new { Id = i, label = $"OtherLabel{i}" }));

    var res = stream1.UnionAll("merge with stream 2", stream2);
    // ...
}
```

:::info

`UnionAll` is a concatenation; it is the least performant way to merge two streams. Unlike in SQL, `Union` is faster than `UnionAll` because `UnionAll` of ETL.NET concatenates both streams. So while the first steam to complete, it needs to store every event of the second stream before emitting them once the first stream is completed.

:::

## Substract

`Substract` remove elements from a stream depending on what is in another stream. The criteria is based on the comparison of 2 keys.

```cs
private static void DefineProcess(ISingleStream<string> contextStream)
{
    var stream1 = contextStream
        .CrossApply("create values from enumeration", ctx => Enumerable
            .Range(1, 100)
            .Select(i => new Tmp1 { Id = i, Label = $"Label{i}" }));
    var stream2 = contextStream
        .CrossApply("create values from enumeration2", ctx => Enumerable
            .Range(1, 8)
            .Select(i => new Tmp1 { Id = i, Label = $"OtherLabel{i}" }));

    // don't issue payload from stream1 if the stream2 emits a payload that contains the same `Id`
    var res = stream1.Substract("merge with stream 2", stream2, i => i.Id, i => i.Id);
    // ...
}
```

If both streams are sorted the way is more straight forward:

```cs
private static void DefineProcess(ISingleStream<string> contextStream)
{
    var stream1 = contextStream
        .CrossApply("create values from enumeration", ctx => Enumerable
            .Range(1, 100)
            .Select(i => new Tmp1 { Id = i, Label = $"Label{i}" }))
        .EnsureSorted("ensure it is sorted on Id", i => new { i.Id });
    var stream2 = contextStream
        .CrossApply("create values from enumeration2", ctx => Enumerable
            .Range(1, 8)
            .Select(i => new Tmp1 { Id = i, Label = $"OtherLabel{i}" }))
        .EnsureKeyed("ensure it is keyed on Id2", i => new { i.Id });

    // don't issue payload from stream1 if the stream2 emits a payload that contains the same `Id`
    var res = stream1.Substract("merge with stream 2", stream2);
    // ...
}
```

Having sorted streams that are already sorted make the operator *much* faster. But sorting it before the operator doesn't make sense as this sort can be very heavy. It makes sense when retrieving sorted data from the database for example.

## Correlate to single/many

The whole correlation mechanism is described in the [normalization recipe](docs/recipes/normalize).
