---
sidebar_position: 4
---

# Flat files

[Handling files in ETL.NET](/docs/recipes/dealWithFiles) involves manipulating streams of payloads implementing the interface `IFileValue`.

This recipe is about dealing with flat files thanks to the dedicated extension.

The first step is top add a reference to `Paillave.EtlNet.TextFile`:

```sh
dotnet add package Paillave.EtlNet.FileSystem
```


## Mapping

Whether it is necessary to read or write a flat file, the first step is to define a mapping.

## Column delimited

```cs
var myMapper = FlatFileDefinition.Create(i => new
{
    Email = i.ToColumn("email"),
    FirstName = i.ToColumn("first name"),
    LastName = i.ToColumn("last name"),
    DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
    Reputation = i.ToNumberColumn<int?>("reputation", ".")
}).IsColumnSeparated(',');
```

This mapping permits to read or write a file that looks like the following:

```csv
email,first name,last name,date of birth,reputation
hello@coucou.org,"first,name",last name,2000-01-26,12
john@doe.org,John,Doe,1960-06-12,10
```

:::info

When it is about reading, the order of fields in the mapping doesn't matter.

:::

This mapping can be achieved this way as well:

```cs
var myMapper = FlatFileDefinition.Create(i => new
{
    Email = i.ToColumn(0),
    FirstName = i.ToColumn(1),
    LastName = i.ToColumn(2),
    DateOfBirth = i.ToDateColumn(3, "yyyy-MM-dd"),
    Reputation = i.ToNumberColumn<int?>(4, ".")
}).IsColumnSeparated(',');
```

## Fixed width columns

```cs
var myMapper = FlatFileDefinition.Create(i => new
{
    Email = i.ToColumn(0, 30),
    FirstName = i.ToColumn(1, 30),
    LastName = i.ToColumn(2, 30),
    DateOfBirth = i.ToDateColumn(3, 10, "yyyy-MM-dd"),
    Reputation = i.ToNumberColumn<int?>(4, 2, ".")
}).IgnoreFirstLines(1);
```

This mapping permits to read or write a file that looks like the following:

```csv
hello@coucou.org              first,name                    last name2000-01-2612
john@doe.org                  John                          Doe      1960-06-1210
```

:::info

:construction: Check tutorial, the essential is documented. Detailed documentation is under construction.

:::
