---
sidebar_position: 4
---

# Deal with databases

ETL.NET official extensions for database permit to save correlated or not correlated using a smart upsert method.

## Without Entity Framework

ETL.NET official extension to access SQL server without Entity Framework is `Paillave.EtlNet.SqlServer`.

### Setup the connection without Entity Framework

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

### Read without Entity Framework

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

### Lookup without Entity Framework

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

### Save without Entity Framework

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

### Execute a SQL process for every row

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

## Using Entity Framework

ETL.NET official extension to access databases with Entity Framework is `Paillave.EtlNet.EntityFrameworkCore`.

Most of things related to Sql Server extension have their equivalent in the Entity Framework extension.

### Setup Entity Framework connection

As Entity Framework needs a connection as well, it will need to be injected in the same way than above in the process:

```cs
var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
using (var dbCtx = new SimpleTutorialDbContext(args[1]))
{
    var executionOptions = new ExecutionOptions<string>
    {
        Resolver = new SimpleDependencyResolver().Register<DbContext>(dbCtx),
    };
    var res = await processRunner.ExecuteAsync(args[0], executionOptions);
}
```

Check [here](/docs/recipes/useExternalData) to have more details about dependency injection.

### Read using Entity Framework

The operator `EfCoreSelect` provides a wrapper to get an entity set along with the current value of the stream for an `IQueryable` to be returned:

```cs
contextStream
    .EfCoreSelect("get posts", (o, row) => o
        .Set<Post>()
        .Where(i => i.Title.Contains(row.TitleCriteria)))
    .Do("show value on console", i => Console.WriteLine(i.Title));
```

Getting one single value per event issued works in a similar way:

```cs
postStream
    .EfCoreSelectSingle("get corresponding authors", (o, row) => o
        .Set<Author>()
        .Where(i => i.Id == row.AuthorId))
    .Do("show value on console", i => Console.WriteLine($"{i.Name}"));
```

### Lookup with Entity Framework

What is mentioned above get one item per event, but it will always run a query per event, and it is hardly possible to combine the output with the input.

If all this is an issue the way to go is the proper lookup system dedicated for Entity Framework `EfCoreLookup`.

```cs
postStream
    .EfCoreLookup("get related authors", o => o
        .Set<Author>()
        .On(i => i.AuthorId, i => i.Id)
        .Select((l, r) => new { Post = l, Author = r }))
    .Do("show value on console", i => Console.WriteLine($"{i.Post.Title} ({i.Author.Name})"));
```

By default `EfCoreLookup` takes the full target dataset and keeps it in memory after making a dictionary out of it based on the targeted key. In some situations, the target dataset is too big to be stored in memory. So the default behavior is **wrong**. There are two solution for this.

The first solution can be used in case of only a subset of the target dataset is necessary. Here is how to do:

```cs {3}
postStream
    .EfCoreLookup("get related authors", o => o
        .Query(o => o.Set<Author>().Where(a => a.TypeId == 6))
        .On(i => i.AuthorId, i => i.Id)
        .Select((l, r) => new { Post = l, Author = r }))
    .Do("show value on console", i => Console.WriteLine($"{i.Post.Title} ({i.Author.Name})"));
```

The second solution suits cases when there is not specific subset in the target dataset. The principle is that at each row, the lookup tries to get the related target item is in the cache. If it is not in the cache it will query it against the database, store in the cache, and return it. To activate this behavior, `NoCacheFullDataset` must be set after the `Select`:

```cs {6}
postStream
    .EfCoreLookup("get related authors", o => o
        .Set<Author>()
        .On(i => i.AuthorId, i => i.Id)
        .Select((l, r) => new { Post = l, Author = r })
        .NoCacheFullDataset())
    .Do("show value on console", i => Console.WriteLine($"{i.Post.Title} ({i.Author.Name})"));
```

Of course, both solutions can be applied together:

```cs {3,6}
postStream
    .EfCoreLookup("get related authors", o => o
        .Query(o => o.Set<Author>().Where(a => a.TypeId == 6))
        .On(i => i.AuthorId, i => i.Id)
        .Select((l, r) => new { Post = l, Author = r })
        .NoCacheFullDataset())
    .Do("show value on console", i => Console.WriteLine($"{i.Post.Title} ({i.Author.Name})"));
```

It is also possible to control the size of the cache:

```cs {7}
postStream
    .EfCoreLookup("get related authors", o => o
        .Set<Author>()
        .On(i => i.AuthorId, i => i.Id)
        .Select((l, r) => new { Post = l, Author = r })
        .NoCacheFullDataset()
        .CacheSize(500))
    .Do("show value on console", i => Console.WriteLine($"{i.Post.Title} ({i.Author.Name})"));
```

Last but not least, in a context of a normalization, the lookup can permit to create the corresponding target item if not found in the target dataset:

```cs {6}
postStream
    .EfCoreLookup("get related authors", o => o
        .Set<Author>()
        .On(i => i.AuthorId, i => i.Id)
        .Select((l, r) => new { Post = l, Author = r })
        .CreateIfNotFound(p => new Author { Name = $"Name {p.AuthorId}" }))
    .Do("show value on console", i => Console.WriteLine($"{i.Post.Title} ({i.Author.Name})"));
```

:::important

In the context of a normalization, if all the data comes from the same source, using lookup operators like `Lookup` or `EfCoreLookup` or even `LeftJoin` is not the best way to go. Use the [correlation system](/docs/recipes/normalize) for this.

:::

### Save using Entity Framework

Except the fact you can she the same exact context between an application and the ETL process, except the fact no mapping is necessary as it is already done in the definition of the context, using ETL.NET Entity Framework extensions offers a very fast bulkload for any use case plus some extra features.

The upsert will also get all the computed/readonly fields at database side and update the row with it.

The operator `EfCoreSave` makes an upsert with Entity Framework by using the primary key that is defined in the database context:

```cs {3}
stream 
    .Select("create author instance", i => new Author { Email = i.Email, Name = i.Author })
    .EfCoreSave("save authors")
    .Do("show output id on console", i => Console.WriteLine(i.Id));
```

To make an upsert using a pivot key that is different than the primary key the option `SeekOn` must be mentioned:

```cs {3,4}
stream 
    .Select("create author instance", i => new Author { Email = i.Email, Name = i.Author })
    .EfCoreSave("save authors", o => o
        .SeekOn(i => i.Email))
    .Do("show output id on console", i => Console.WriteLine(i.Id));
```

I can also be written the following way to permit the pivot key to be composed with several fields:

```cs {4}
stream 
    .Select("create author instance", i => new Author { Email = i.Email, Name = i.Author })
    .EfCoreSave("save authors", o => o
        .SeekOn(i => new { i.Email }))
    .Do("show output id on console", i => Console.WriteLine(i.Id));
```

It can happen that an object can have several several business keys. We can imagine for example, a author has a social security number as an identifier, and an author reference. We want the system to get the author based on his social security number first, and if it is not found this way, it tries to use the author reference instead. `AlternativelySeekOn` option is meant to accomplish this purpose:

```cs {5}
stream 
    .Select("create author instance", i => new Author { Email = i.Email, Name = i.Author })
    .EfCoreSave("save authors", o => o
        .SeekOn(i => new { i.SocialSecurityNumber })
        .AlternativelySeekOn(i => { i.AuthorReference }))
    .Do("show output id on console", i => Console.WriteLine(i.Id));
```

To insert only rows that don't exist already but still by retrieving the computed/readonly fields at database side and set it on the local row, the option `DoNotUpdateIfExists` must be used:

```cs {5}
stream 
    .Select("create author instance", i => new Author { Email = i.Email, Name = i.Author })
    .EfCoreSave("save authors", o => o
        .SeekOn(i => new { i.Email })
        .DoNotUpdateIfExists())
    .Do("show output id on console", i => Console.WriteLine(i.Id));
```

To insert ALL the rows into the database with no exception, use `InsertOnly` option:

```cs {3}
stream 
    .EfCoreSave("save traces", o => o
        .InsertOnly())
    .Do("show trace id on console", i => Console.WriteLine(i.Id));
```

To save an object that is not the actual payload of events of the stream, the entity to save must be specified with the option `Entity`:

```cs {3}
stream 
    .EfCoreSave("save authors", o => o
        .Entity(i=> new Author { Email = i.Email, Name = i.Author })
        .SeekOn(i => new { i.SocialSecurityNumber }))
    .Do("show output id on console", i => Console.WriteLine(i.Id));
```

The output stream issues payload that are the entity updated accordingly to what is in the database. To get an output that is a combination of this entity and of the initial payload, use the option `Output`:

```cs {5,6}
stream 
    .EfCoreSave("save authors", o => o
        .Entity(i=> new Author { Email = i.Email, Name = i.Author })
        .SeekOn(i => new { i.SocialSecurityNumber })
        .Output((r, e) => new { AuthorRow = r, SavedEntity = e }))
    .Do("show output id on console", i => Console.WriteLine(i.SavedEntity.Id));
```

:::important

**By default, save operation of the EF Core extension doesn't fully use EF Core to make saves**. It uses metadata of the `DbContext` to inspect the mapping, computed columns, primary keys, converters and TPH inheritance specifications. With these metadata, the extension makes a bulkload and runs suitable `MERGE` or `INSERT` SQL statements. **In this default mode, the extension works only on SQL Server databases**. Of course, this is way faster that actually fully using EF Core. 

This way to use the extension takes in charge every capabilities of EF Core except for 2 exceptions:

- Owned object (`OwnsOne` or `OwnsMany`)
- Table-per-Type inheritance (Table-per-Hierarchy is totally taken in charge)

On this version of the extension, **this default mode needs the connection to be able to create and delete tables in the database**. Later on, temporary tables may be used instead, and this requirement won't applicable anymore.

To fully use EF Core, and therefore being able to work with any database that EF Core takes in charge, set the option `WithMode` to `SaveMode.EntityFrameworkCore`

```cs {5}
stream 
    .EfCoreSave("save authors", o => o
        .Entity(i=> new Author { Email = i.Email, Name = i.Author })
        .SeekOn(i => new { i.SocialSecurityNumber })
        .WithMode(SaveMode.EntityFrameworkCore))
    .Do("show output id on console", i => Console.WriteLine(i.Id));
```

:::