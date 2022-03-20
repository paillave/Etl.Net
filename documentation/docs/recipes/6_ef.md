---
sidebar_position: 6
---

# Data access with entity framework

ETL.NET official extensions for database permit to save correlated or not correlated using a smart upsert method.

ETL.NET official extension to access databases with Entity Framework is `Paillave.EtlNet.EntityFrameworkCore`.

Most of things related to Sql Server extension have their equivalent in the Entity Framework extension.

## Setup the connection

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

## Read

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

## Lookup

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

## Save

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

## Delete

TODO: to be done

## Update

TODO: to be done
