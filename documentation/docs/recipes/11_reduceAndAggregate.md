---
sidebar_position: 11
---

# Reduce/aggregate and distinct

## Distinct

`Distinct` Permits to remove duplicates based on a given key.

```cs {9}
contextStream
    .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100)
        .Select(i => new
        {
            OutputId = i % 11,
            Label = $"{ctx}{i % 11}",
            Description = (i % 5 == 0) ? null : $"Description {i}"
        }))
    .Distinct("Distinct ", i => i.OutputId)
    .Do("print file name to console", i => Console.WriteLine(i));
```

Input stream:

| OutputId | Label | Description |
| - | - | - |
| 1 | Label 1 | Description 1 |
| 2 | | Description 2 |
| 3 | | Description 3 |
| 4 | Label 4 | Description 4 |
| 5 | Label 5 | |
| 6 | Label 6 | Description 6 |
| 0 | Label 0 | Description 0 |
| 1 | Label 1 | |
| 2 | Label 2 | Description 2 |
| 3 | Label 3 | Description 3 |
| 4 | | Description 4 |
| 5 | | Description 5 |
| 6 | Label 6 | Description 6 |

The regular `Distinct` provides this result:

| OutputId | Label | Description |
| - | - | - |
| 1 | Label 1 | Description 1 |
| 2 | | Description 2 |
| 3 | | Description 3 |
| 4 | Label 4 | Description 4 |
| 5 | Label 5 | |
| 6 | Label 6 | Description 6 |
| 0 | Label 0 | Description 0 |

Sometimes, from bad input files can fill a value for a column but not always. It is a usual request in such a context to take in consideration only not null value instead of the first found record as is. Here the output stream we would like to reach:

| OutputId | Label | Description |
| - | - | - |
| 1 | Label 1 | Description 1 |
| 2 | Label 2 | Description 2 |
| 3 | Label 3 | Description 3 |
| 4 | Label 4 | Description 4 |
| 5 | Label 5 | Description 5 |
| 6 | Label 6 | Description 6 |
| 0 | Label 0 | Description 0 |

Obtaining this result is made by setting `true` the second optional parameter of `Distinct` operator.

```cs {9}
contextStream
    .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100)
        .Select(i => new
        {
            OutputId = i % 11,
            Label = $"{ctx}{i % 11}",
            Description = (i % 5 == 0) ? null : $"Description {i}"
        }))
    .Distinct("Distinct ", i => i.OutputId, true)
    .Do("print file name to console", i => Console.WriteLine(i));
```

## Group By

In the [create several files recipe](docs/recipes/createFiles) `GroupBy` was used to group payloads in a sub process per key. But the `GroupBy` operator can be used to group payload in a list per key.

```cs {9}
contextStream
    .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100)
        .Select(i => new
        {
            OutputId = i % 11,
            Label = $"{ctx}{i}",
            Description = (i % 5 == 0) ? null : $"Description {i}"
        }))
    .GroupBy("group by OutputId", i => i.OutputId)
    .Do("print file name to console", i => Console.WriteLine($"{i.Key}: {i.Aggregation.Count} items"));
```

## Aggregate

`GroupBy` simply groups payloads in lists or in sub processes. `Aggregate` permits to make any free action per group of payload. It can make more things than `GroupBy` but it is lest straight forward to use.

```cs {10-17}
contextStream
    .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100)
        .Select(i => new
        {
            Id = i,
            OutputId = i % 11,
            Label = $"{ctx}{i}",
            Description = (i % 5 == 0) ? null : $"Description {i}"
        }))
    .Aggregate("aggregate by OutputId",
        i => i.OutputId,
        i => new { Key = i.OutputId, Ids = new List<int>() },
        (a, v) =>
        {
            a.Ids.Add(v.Id);
            return a;
        })
    .Do("print file name to console", i => Console.WriteLine($"{i.Key}: {i.Aggregation.Ids.Count} items"));
```

## Pivot

`Pivot` makes several aggregation of values on a single output occurrence. Like the `PIVOT` of SQL, or the pivot table of excel.

```cs
contextStream
    .CrossApply("create values from enumeration", ctx => Enumerable.Range(1, 100)
        .Select(i => new
        {
            Id = i,
            OutputId = i % 3,
            Label = $"{ctx}{i}",
            Description = (i % 5 == 0) ? null : $"Description {i}"
        }))
    .Pivot("pivot values", i => i.OutputId, i => new
    {
        Count = AggregationOperators.Count(),
        Count0 = AggregationOperators.Count().For(i.OutputId == 0),
        Count1 = AggregationOperators.Count().For(i.OutputId == 1),
        Count2 = AggregationOperators.Count().For(i.OutputId == 2)
    })
    .Do("print file name to console", i => Console.WriteLine($"{i.Key}: Count={i.Aggregation.Count}, Count0={i.Aggregation.Count0}, Count1={i.Aggregation.Count1}, Count2={i.Aggregation.Count2}"));
```
## ToList

TODO