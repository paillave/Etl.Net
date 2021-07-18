---
sidebar_position: 2
---

# ETL process

![Read Process](/img/integrate-database-library-graphs.svg)

The definition of the ETL process is done in the method `DefineProcess`. The following will be the implementation of this method.

## List zip files

By using extensions from `Paillave.Etl.FileSystem`, we will recursively list all the zip files in the root folder that was transmitted when triggering the execution:

```cs {2-3}
contextStream
    .CrossApplyFolderFiles("list all required files", "*.zip", true)
    .Do("display zip file name on console", i => Console.WriteLine(i.Name));
```

## Extract the right files from zip files

By using extensions from `Paillave.Etl.Zip`, we will recursively list all the csv files contained in all the enumerated zip files:

```cs {3-4}
contextStream
    .CrossApplyFolderFiles("list all required files", "*.zip", true)
    .CrossApplyZipFiles("extract files from zip", "*.csv")
    .Do("display extracted csv file name on console", i => Console.WriteLine(i.Name));
```

## Parse csv files

By using extensions from `Paillave.Etl.TextFile`, we will parse every csv file that has been unzipped:

```cs {4-12}
contextStream
    .CrossApplyFolderFiles("list all required files", "*.zip", true)
    .CrossApplyZipFiles("extract files from zip", "*.csv")
    .CrossApplyTextFile("parse file", FlatFileDefinition.Create(i => new
    {
        Email = i.ToColumn("email"),
        FirstName = i.ToColumn("first name"),
        LastName = i.ToColumn("last name"),
        DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
        Reputation = i.ToNumberColumn<int?>("reputation", ".")
    }).IsColumnSeparated(','))
    .Do("display parsed person email on console", i => Console.WriteLine(i.Email));
```

## Setup the connection

By using `System.Data.SqlClient`, we create a connection to the database and we will inject it into the ETL process when triggering it.

The extension that needs to operate with the database will get its connection through the DI setup here.

```cs {4-11,13}
static async Task Main(string[] args)
{
    var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
    using (var cnx = new SqlConnection(args[1]))
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

The `Distinct` operator, in its common usage will ignore any recurring input based on the given key (the key can be an anonymous type with several properties).

```cs {12}
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

We will save everything in the database using the following and very common rules during the integration of data in a database:

- We will exclude duplicates on the business key (the email)
- We will make an upsert in the target table based on the business key (the email)
- The object that is upserted is updated with the value of every field of the table, taking in consideration all the computed values at database level like the Id

By using `Paillave.Etl.SqlServer`, save every occurrence in the database, and get updates from so that every object is exactly like it is in the table after the upsert.

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
using Paillave.Etl.FileSystem;
using Paillave.Etl.Zip;
using Paillave.Etl.TextFile;
using Paillave.Etl.SqlServer;
using System.Data.SqlClient;
using Paillave.Etl.Core;

namespace SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            using (var cnx = new SqlConnection(args[1]))
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
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            contextStream
                .CrossApplyFolderFiles("list all required files", "*.zip", true)
                .CrossApplyZipFiles("extract files from zip", "*.csv")
                .CrossApplyTextFile("parse file", 
                    FlatFileDefinition.Create(i => new Person
                    {
                        Email = i.ToColumn("email"),
                        FirstName = i.ToColumn("first name"),
                        LastName = i.ToColumn("last name"),
                        DateOfBirth = i.ToDateColumn("date of birth", "yyyy-MM-dd"),
                        Reputation = i.ToNumberColumn<int?>("reputation", ".")
                    }).IsColumnSeparated(','))
                .Distinct("exclude duplicates based on the Email", i => i.Email)
                .SqlServerSave("upsert using Email as key and ignore the Id", 
                    "dbo.Person", 
                    p => p.Email, 
                    p => p.Id)
                .Select("define row to report", i => new { i.Email, i.Id })
                .ToTextFileValue("write summary to file", 
                    "report.csv", 
                    FlatFileDefinition.Create(i => new
                    {
                        Email = i.ToColumn("Email"),
                        Id = i.ToNumberColumn<int>("new or existing Id", ".")
                    }).IsColumnSeparated(','))
                .WriteToFile("save log file", i => i.Name);
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
    }
}
```
