---
sidebar_position: 5
---

# Data access native SQL Server

ETL.NET official extension to access SQL server without Entity Framework is `Paillave.EtlNet.SqlServer`.

ETL.NET official extensions for database permit to save correlated or not correlated using a smart upsert method.

## Setup the connection

Obviously, for Sql Server operators to work, they need a connection to the database. [Dependency injection](/docs/recipes/useExternalData) must be used to inject the connection in the process when triggering the process with `ProcessRunner`.

```cs
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
using (var cnx = new SqlConnection(args[1]))
{
    cnx.Open();
    var executionOptions = new ExecutionOptions<string>
    {
        Resolver = new SimpleDependencyResolver().Register(cnx),
    };
    var res = await processRunner.ExecuteAsync(args[0], executionOptions);
}
```

Check [here](/docs/recipes/useExternalData) to have more details about dependency injection.

## Read

Reading data from the database is based on a `CrossApply` operator type. This means that the operation will execute the query and issue new rows **for each event of the input stream**.

Get data with no criteria using a class that matches **exactly** the structure of the returned dataset:

```cs
contextStream
    .CrossApplySqlServerQuery("get people", o => o
        .FromQuery("select p.* from dbo.Person as p")
        .WithMapping<Person>())
    .Do("show people on console", i => Console.WriteLine($"{i.FirstName} {i.LastName}: ${i.DateOfBirth:yyyy-MM-dd}"));
```

Here is how to do to accomplish the same if working using a class that doesn't match the structure of even with an anonymous type:

```cs
contextStream
    .CrossApplySqlServerQuery("get people", o => o
        .FromQuery("select p.* from dbo.Person as p")
        .WithMapping(i => new
        {
            Name = i.ToColumn("FirstName"),
            Birthday = i.ToDateColumn("DateOfBirth")
        }))
    .Do("show people on console", i => Console.WriteLine($"{i.Name}: ${i.Birthday:yyyy-MM-dd}"));
```

To apply a criteria for each execution, the input payload must contain properties with a name and a type that corresponds the necessary SQL variable that is used in the SQL statement:

```cs
contextStream
    .Select("build criteria", i => new { Reputation = 345 })
    .CrossApplySqlServerQuery("get people", o => o
        .FromQuery("select p.* from dbo.Person as p where p.Reputation = @Reputation")
        .WithMapping(i => new
        {
            Name = i.ToColumn("FirstName"),
            Birthday = i.ToDateColumn("DateOfBirth")
        }))
    .Do("show people on console", i => Console.WriteLine($"{i.Name}: ${i.Birthday:yyyy-MM-dd}"));
```

## Lookup

To make a lookup, extensions for Sql Server don't provide any operator out of the box. The work around is to use the in memory lookup of ETL.NET core.

```cs
var authorStream = contextStream
    .CrossApplySqlServerQuery("get authors", o => o
        .FromQuery("select a.* from dbo.Author as a")
        .WithMapping(i => new
        {
            Id = i.ToNumberColumn<int>("Id"),
            Name = i.ToColumn("Name"),
            Reputation = i.ToNumberColumn<int>("Reputation")
        }));

postStream
    .Lookup("get related author", authorStream,
        l => l.AuthorId,
        r => r.Id,
        (l, r) => new { Post = l, Author = r })
    .Do("show value on console", i => Console.WriteLine($"{i.Post.Title} ({i.Author.Name})"));

```

:::warning

Keep in mind that the lookup operator is compelled to wait for the whole target stream to be completed to actually start the actual lookup operation. Of course, **at the same time it is storing the full right stream in memory, it does the same with the left stream the right one is completed**. To avoid this, consider the `LeftJoin` operator, but this one need both streams to be sorted on the pivot value.

:::

As mentioned in the previous warning, the `Lookup` operator is not without problem, and the alternative can be the `LeftJoin` of both input stream are properly sorted. With the two stream sorted on the pivot key, just the latest row of both streams will be stored in memory. If any stream is already sorted, it makes absolutely no sense to sort it. This is where `EnsureSorted` comes. It permits to check that the stream is properly sorted. If it is not properly sorted, and error will be raised and the process will stop. For the target stream, `EnsureKeyed` will make the same but will check that there is no duplicate either.

This way, the process will be as fast and memory saving as it can be, even with billions rows.

```cs {3,10,13,14}
var authorStream = contextStream
    .CrossApplySqlServerQuery("get authors", o => o
        .FromQuery("select a.* from dbo.Author as a order by a.Id")
        .WithMapping(i => new
        {
            Id = i.ToNumberColumn<int>("Id"),
            Name = i.ToColumn("Name"),
            Reputation = i.ToNumberColumn<int>("Reputation")
        }))
    .EnsureKeyed("ensure authors are sorted by Id with no duplicate without actually sorting it", i => i.Id);

postStream
    .EnsureSorted("ensure posts are sorted by AuthorId without actually sorting it", i => i.AuthorId);
    .LeftJoin("get related author", authorStream,
        l => l.AuthorId,
        r => r.Id,
        (l, r) => new { Post = l, Author = r })
    .Do("show value on console", i => Console.WriteLine($"{i.Post.Title} ({i.Author.Name})"));
```

Look [here](/docs/recipes/linkStreams) for more details about how to combine streams.

## Save

Saving data in the database works with the operator `SqlServerSave`. For each row, this operator does the following:

- Tries the get an occurrence with the same pivot value
- If not found create a new row by excluding values not to save (if any). If found, update the retrieved row - in the same manner.
- Save the updated or created row
- Get every value as it is in the database to update the current payload with it

:::note

The payload to be saved must be a **class** that has a structure that must match the target table.

:::

Here is how to save a list of people that are identified by their `Email` in a table that has a primary key `Id` that is auto generated:

```cs
peopleStream
    .SqlServerSave("upsert using Email as key and ignore the Id", o => o
        .ToTable("dbo.Person")
        .SeekOn(p => p.Email)
        .DoNotSave(p => p.Id));
```

The property `Id` of the person will be updated with the `Id` from the update or created row.

If the business key is on several field, and/or if there are several column that are computed at database level, the upsert must be done this way:

```cs
peopleStream
    .SqlServerSave("upsert using Email as key and ignore the Id", o => o
        .ToTable("dbo.Person")
        .SeekOn(p => new { p.DateOfBirth, p.Number })
        .DoNotSave(p => new { p.Id, p.Timestamp }));
```

Of course, making a regular insert with no update is possible:

```cs
traceStream
    .SqlServerSave("insert in a trace table", o => o
        .ToTable("dbo.Trace")
        .DoNotSave(p => p.Id));
```

If the class has the same name than the target table and that the target table is in the default schema (in 99% of cases: `dbo`), it is not necessary to mention the target table explicitly:

```cs
traceStream.SqlServerSave("insert in a trace table", o => o.DoNotSave(p => p.Id));
```

:::important

For the moment, saving with the SQL extension executes an **upsert sql statement for each row** behind the scenes. It doesn't proceed using bulk load with one single merge statement for a full bulk of rows. To have a bulkload, the extension using Entity Framework must be used instead.

:::

## Execute a SQL process for every row

Calling an arbitrary SQL statement or a stored procedure is made with the operator `ToSqlCommand`. It is given a statement with parameters that must have their equivalent in properties of the payload of the stream.

Here, the goal is to change the reputation for the one who have 345 and 45 as a current reputation:

```cs
contextStream
    .CrossApply("build criteria", i => new[] 
    {
        new { Reputation = 345, NewReputation = 346 },
        new { Reputation = 45, NewReputation = 201 }
    })
    .ToSqlCommand("update reputation", 
        "update p set Reputation = @NewReputation from dbo.Person as p where p.Reputation = @Reputation");
```

`ToSqlCommand` always returns the input events as is. The following can be done for example:

```cs
contextStream
    .CrossApply("build criteria", i => new[] 
    {
        new { Reputation = 345, NewReputation = 346 },
        new { Reputation = 45, NewReputation = 201 }
    })
    .ToSqlCommand("update reputation", 
        "update p set Reputation = @NewReputation from dbo.Person as p where p.Reputation = @Reputation")
    .ToSqlCommand("update reputation like before", 
        "update p set Reputation = @Reputation from dbo.Person as p where p.Reputation = @NewReputation");
```
