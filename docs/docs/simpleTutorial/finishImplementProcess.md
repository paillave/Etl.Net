---
sidebar_position: 5
---

# Part 4: Implement the process to save data

So far, we implemented a process that shows some data on the console.

We will amend it to save everything in the database using the following and very common rules during the integration of data in a database:

- We will exclude duplicates on the business key (the email)
- We will make an upsert in the target table based on the business key (the email)
- The object that is upserted is updated with the value of every field of the table, taking in consideration all the computed values at database level like the Id

## Setup the connection

By using `System.Data.SqlClient`, we create a connection to the database and we will inject it into the ETL process when triggering it.

The extension that needs to operate with the database will get its connection through the DI setup here.

```cs {4-11,13}
static async Task Main(string[] args)
{
    var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
    using (var cnx = new SqlConnection(@"Server=localhost,1433;Database=SimpleTutorial;user=SimpleTutorial;password=TestEtl.TestEtl;MultipleActiveResultSets=True"))
    {
        cnx.Open();
        var executionOptions = new ExecutionOptions<string>
        {
            Resolver = new SimpleDependencyResolver().Register(cnx)
        };
        var res = await processRunner.ExecuteAsync(args[0], executionOptions);
        Console.Write(res.Failed ? "Failed" : "Succeeded");
    }
}
```

## Create a class to replace the anonymous type

This class is necessary for 2 reasons:

- We want to retrieve the Id for every record that is upserted and it is not in the input file
- The object will be updated by the process so it cannot be anonymous

:::note

The structure of the class must match the table.

:::

```cs
private class Person
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public int? Reputation { get; set; }
}
```

## Parse csv files using the new class

The flat file parser works with concrete types as well. We will use the new class `Person` instead of an anonymous type:

```cs {4}
contextStream
    .CrossApplyFolderFiles("list all required files", "*.zip", true)
    .CrossApplyZipFiles("extract files from zip", "*.csv")
    .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new Person
        {
            Email = i.ToColumn("email"),
            FirstName = i.ToColumn("first name"),
            LastName = i.ToColumn("last name"),
            DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
            Reputation = i.ToNumberColumn<int?>("reputation", ".")
        }).IsColumnSeparated(','))
    .Do("display parsed person email on console", i => Console.WriteLine(i.Email));
```

## Ensure there are no duplicates based on the email

The `distinct` operator, in its common usage will ignore any recurring input based on the given key (the key can be an anonymous type with several properties).

```cs {4}
contextStream
    .CrossApplyFolderFiles("list all required files", "*.zip", true)
    .CrossApplyZipFiles("extract files from zip", "*.csv")
    .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new Person
        {
            Email = i.ToColumn("email"),
            FirstName = i.ToColumn("first name"),
            LastName = i.ToColumn("last name"),
            DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
            Reputation = i.ToNumberColumn<int?>("reputation", ".")
        }).IsColumnSeparated(','))
    .Distinct("exclude duplicates", i => i.Email)
    .Do("display parsed person email on console", i => Console.WriteLine(i.Email));
```

## Upsert each occurrence in the target table

By using `Paillave.Etl.SqlServer.Extensions`, save every occurrence in the database, and get updates from so that every object is exactly like it is in the table after the upsert.

```cs {13-14}
contextStream
    .CrossApplyFolderFiles("list all required files", "*.zip", true)
    .CrossApplyZipFiles("extract files from zip", "*.csv")
    .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new Person
        {
            Email = i.ToColumn("email"),
            FirstName = i.ToColumn("first name"),
            LastName = i.ToColumn("last name"),
            DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
            Reputation = i.ToNumberColumn<int?>("reputation", ".")
        }).IsColumnSeparated(','))
    .Distinct("exclude duplicates", i => i.Email)
    .SqlServerSave("save in DB", "dbo.Person", p => p.Email, p => p.Id)
    .Do("display ids on console", i => Console.WriteLine(i.Id));
```

## Full source

This piece of program is a typical process to make a reliable upsert of the content of every zipped csv file in a folder.

```cs
using System;
using System.Threading.Tasks;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Extensions;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;
using Paillave.Etl;
using Paillave.Etl.TextFile;
using Paillave.Etl.TextFile.Core;
using Paillave.Etl.SqlServer.Extensions;
using System.Data.SqlClient;
using Paillave.Etl.Core;

namespace Paillave.Etl.SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            using (var cnx = new SqlConnection(@"Server=localhost,1433;Database=SimpleTutorial;user=SimpleTutorial;password=TestEtl.TestEtl;MultipleActiveResultSets=True"))
            {
                cnx.Open();
                var executionOptions = new ExecutionOptions<string>
                {
                    Resolver = new SimpleDependencyResolver().Register(cnx)
                };
                var res = await processRunner.ExecuteAsync(args[0], executionOptions);
                Console.Write(res.Failed ? "Failed" : "Succeeded");
            }
        }
        private class Person
        {
            public int Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public int? Reputation { get; set; }
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApplyFolderFiles("list all required files", "*.zip", true)
                .CrossApplyZipFiles("extract files from zip", "*.csv")
                .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new Person
                    {
                        Email = i.ToColumn("email"),
                        FirstName = i.ToColumn("first name"),
                        LastName = i.ToColumn("last name"),
                        DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
                        Reputation = i.ToNumberColumn<int?>("reputation", ".")
                    }).IsColumnSeparated(','))
                .Distinct("exclude duplicates", i => i.Email)
                .SqlServerSave("save in DB", "dbo.Person", p => p.Email, p => p.Id)
                .Do("display ids on console", i => Console.WriteLine(i.Id));
        }
    }
}
```
