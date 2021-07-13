---
sidebar_position: 4
---

# Part 3: Implement the process to read files

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

## Full source at this stage

This piece of program show on the console of every email found in every file

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
using Paillave.Etl.Core;

namespace Paillave.Etl.SimpleTutorial
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            var res = await processRunner.ExecuteAsync(args[0]);
            Console.Write(res.Failed ? "Failed" : "Succeeded");
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
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
        }
    }
}
```
