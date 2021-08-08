---
sidebar_position: 4
---

# Interaction with databases

ETL.NET official extensions for database permit to save correlated or not correlated using a smart upsert method.

## Without Entity Framework

ETL.NET official extension to access SQL server without Entity Framework is `Paillave.Etl.SqlServer`.

### Read without Entity Framework

Reading data from the database is based on a `CrossApply` operator type. This means that the operation will execute the query and issue new rows **for each event of the input stream**.

Get data with no criteria using a class that matches **exactly** the structure of the returned dataset:

```cs
contextStream
    .CrossApplySqlServerQuery("get people", o => o
        .WithQuery("select * from dbo.Person as p")
        .WithMapping<Person>())
    .Do("show people on console", i => Console.WriteLine($"{i.FirstName} {i.LastName}: ${i.DateOfBirth:yyyy-MM-dd}"));
```

To accomplish the same if working using an class that doesn't match the structure of even with an anonymous type:

```cs
contextStream
    .CrossApplySqlServerQuery("get people", o => o
        .WithQuery("select * from dbo.Person as p")
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
        .WithQuery("select * from dbo.Person as p where p.Reputation = @Reputation")
        .WithMapping(i => new
        {
            Name = i.ToColumn("FirstName"),
            Birthday = i.ToDateColumn("DateOfBirth")
        }))
    .Do("show people on console", i => Console.WriteLine($"{i.Name}: ${i.Birthday:yyyy-MM-dd}"));
```

### Save without Entity Framework

## Using Entity Framework

### Read using Entity Framework

### Save using Entity Framework
