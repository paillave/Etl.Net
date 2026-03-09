// using System;
// // using Paillave.Etl.FromConfigurationConnectors;
// using Paillave.Etl.FileSystem;
// using Paillave.Etl.Core;
// using Paillave.Etl.ExecutionToolkit;
// using System.Threading.Tasks;
// using System.IO;
// using Paillave.Etl.FromConfigurationConnectors;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Paillave.Etl.FromConfigurationConnectors;
using Paillave.Etl.Mail;
using Paillave.Etl.GraphApi;

namespace Paillave.Etl.Samples;

class Program
{
    static async Task Main(string[] args)
    {
        var tmp = new GraphApi.GraphApiMessaging(new GraphApiAdapterConnectionParameters
        {
            From = "support@fundprocess.lu",
            FromDisplayName = "FundProcess in Development",
            MaxAttempts = 3,
        });
        tmp.Send(null, "Test subject", "This is the body", false, [new MessageContact
            {
                Email="stephane.royer@fundprocess.lu",
                DisplayName="Recipient Name"
            }]);
        // var tmp = new SmtpMessaging(new MailAdapterConnectionParameters
        // {
        //     From = "support@fundprocess.lu",
        //     FromDisplayName = "FundProcess in Development",
        //     MaxAttempts = 3,
        //     PortNumber = 587,
        //     Server = "smtp-relay.brevo.com",
        //     Ssl = false,
        // });
        // tmp.Send(null, "Test subject", "This is the body", false, [new MessageContact
        //     {
        //         Email="recipient@email.com",
        //         DisplayName="Recipient Name"
        //     }]);
        // await SimplyImport2Async(args);
        // CreateConnectorConfigurationFileSchema();
        // await ConnectorTestAsync(args);
        // await ImportAndCreateFileAsync(args);
        // await ImportAndCreateFileWithConfigAsync(args);
    }
    // private static ConfigurationFileValueConnectorParser CreateConfigurationFileValueConnectorParser() => new(
    //     new FileSystemProviderProcessorAdapter());
    // public static void CreateConnectorConfigurationFileSchema()
    //     => File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "connectorsConfigSchema.json"), CreateConfigurationFileValueConnectorParser().GetConnectorsSchemaJson());

    //         /// <summary>
    //         /// Example 1: simple importation of a set of files
    //         /// </summary>
    //         /// <param name="args"></param>
    //         /// <returns></returns>



    // "FUNDPROCESS_Messaging__Properties__From": "support@fundprocess.lu",
    // "FUNDPROCESS_Messaging__Properties__FromDisplayName": "FundProcess in Development",
    // "FUNDPROCESS_Messaging__Properties__MaxAttempts": "3",
    // "FUNDPROCESS_Messaging__Properties__PortNumber": "587",
    // "FUNDPROCESS_Messaging__Properties__Server": "smtp-relay.brevo.com",
    // "FUNDPROCESS_Messaging__Properties__Ssl": "false",
    // "FUNDPROCESS_Messaging__Type": "Smtp",







    static async Task SimplyImport2Async(string[] args)
    {
        var processRunner = StreamProcessRunner.Create<string[]>(contextStream =>
                    contextStream
                .CrossApply("ca", i => Enumerable.Range(0, 2))
                .SubProcess("sub", i => i
                    .CrossApply("cb", j => Enumerable.Range(0, 10).Select(k => new { j, k }))
                )
                .Do("show on screen", i =>
                {
                    Thread.Sleep(100);
                    Console.WriteLine($"{i.j}-{i.k}");
                }));
        // processRunner.GetDefinitionStructure().OpenEstimatedExecutionPlan();
        var output = await processRunner.ExecuteAsync(args);
    }

















    static async Task SimplyImportAsync(string[] args)
    {


        var configuration = ApplicationConfigurationBuilder.GetConfiguration(args);

        var configurationMessagingProvider = new ConfigurationMessagingProvider(configuration, [
            new SmtpMessagingProvider(),
                new GraphApiMessagingProvider()
        ]);
        var msg = configurationMessagingProvider.GetMessaging("Messaging");






        var configAdapter = new ConfigurationAdapterProvider(
            configuration,
            new ConfigurationFileValueConnectorParser(
                new MailProviderProcessorAdapter(),
                new FileSystemProviderProcessorAdapter(),
                new GraphApiProviderProcessorAdapter()));

        var provider = configAdapter.GetFileValueProvider("MyProvider");
        var processor = configAdapter.GetFileValueProcessor("MyProcessor");

        await foreach (var fileValue in provider.ProvideAsync())
            await foreach (var newFileValue in processor.ProcessAsync(fileValue)) ;

        // processor.Process(new InMemoryFileValue(new MemoryStream(), "TestFile.txt"), i => { }, default);


        var services = new ServiceCollection()
            .AddPooledDbContextFactory<DataAccess.TestDbContext>(options => options.UseSqlServer(@"Server=localhost,1433;Initial Catalog=TestEtl;Persist Security Info=False;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=true;Connection Timeout=300;", options => options
                .CommandTimeout(2000)
                .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)))
            .AddTransient<DbContext>(sp => sp
                .GetRequiredService<IDbContextFactory<DataAccess.TestDbContext>>().CreateDbContext())
            .AddSingleton<IFileValueConnectors>(new FileValueConnectors()
                .Register(new FileSystemFileValueProvider("PTF", "Portfolios", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Portfolios.csv"))
                .Register(new FileSystemFileValueProvider("POS", "Positions", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Positions.csv")))
            .BuildServiceProvider();

        using (var dbCtx = services.GetRequiredService<DbContext>())
            dbCtx.Database.Migrate();

        var executionOptions = new ExecutionOptions<string[]>
        {
            Services = services,
            TraceProcessDefinition = (ts, cs) => ts.Do("Show trace on console", t => Console.WriteLine(t.ToString())),
            // TraceProcessDefinition = DefineTraceProcess,
            // UseDetailedTraces = true // activate only if per row traces are meant to be caught
        };



        var processRunner = StreamProcessRunner.Create<string[]>(TestImport.Import);
        // processRunner.GetDefinitionStructure().OpenEstimatedExecutionPlan();
        var output = await processRunner.ExecuteAsync(args, executionOptions);
    }

    //         /// <summary>
    //         /// Example 2: Import and export files showing the estimated execution plan, the real time evolution of the process, and the actual execution plan
    //         /// </summary>
    //         /// <param name="args"></param>
    //         /// <returns></returns>
    //         static async Task ImportAndCreateFileAsync(string[] args)
    //         {
    //             var processRunner = StreamProcessRunner.Create<string[]>(TestImport2.Import);
    //             var structure = processRunner.GetDefinitionStructure();
    //             // structure.OpenEstimatedExecutionPlan();

    //             // ITraceReporter traceReporter = new AdvancedConsoleExecutionDisplay();
    //             ITraceReporter traceReporter = new SimpleConsoleExecutionDisplay();
    //             var dataAccess = new DataAccess.TestDbContext();
    //             await dataAccess.Database.EnsureCreatedAsync();
    //             // dataAccess.Database.Migrate();
    //             var executionOptions = new ExecutionOptions<string[]>
    //             {
    //                 Connectors = new FileValueConnectors()
    //                     .Register(new FileSystemFileValueProvider("PTF", "Portfolios", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Portfolios.csv"))
    //                     .Register(new FileSystemFileValueProvider("POS", "Positions", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Positions.csv"))
    //                     .Register(new FileSystemFileValueProcessor("OUT", "Result", Path.Combine(Environment.CurrentDirectory, "InputFiles"))),
    //                 Resolver = new SimpleDependencyResolver()
    //                     .Register(dataAccess),
    //                 TraceProcessDefinition = traceReporter.TraceProcessDefinition,
    //             };
    //             traceReporter.Initialize(structure);

    // var res = await processRunner.ExecuteAsync(args, executionOptions);
    // res.OpenActualExecutionPlan();
    // }
    // static async Task ConnectorTestAsync(string[] args)
    // {
    //     var processRunner = StreamProcessRunner.Create<string[]>(i => i
    //         .FromConnector("in", "IN")
    //         .Do("write on console", context =>
    //         {
    //             Console.WriteLine($"Processing {context.Name}");
    //         })
    //     // .ToConnector("out", "OUT")
    //     );
    //     var structure = processRunner.GetDefinitionStructure();
    //     // structure.OpenEstimatedExecutionPlan();

    //     ITraceReporter traceReporter = new SimpleConsoleExecutionDisplay();
    //     // var dataAccess = new DataAccess.TestDbContext();
    //     // await dataAccess.Database.EnsureCreatedAsync();
    //     // dataAccess.Database.Migrate();
    //     var executionOptions = new ExecutionOptions<string[]>
    //     {
    //         Connectors = new FileValueConnectors()
    //             .Register(
    //                 new SftpFileValueProvider("IN", "Input", "Sftp", new SftpAdapterConnectionParameters
    //                 {
    //                     Server = "localhost",
    //                     PortNumber = 22,
    //                     Login = ConsoleAsk("login:"),
    //                     Password = ConsoleAsk("password:"),
    //                     MaxAttempts = 3,
    //                     RootFolder = "stephane",
    //                     // RootFolder = "/stephane/Documents",
    //                 },
    //                 new SftpAdapterProviderParameters
    //                 {
    //                     SubFolder = "Documents",
    //                     // SubFolder = "",
    //                     FileNamePattern = "*"
    //                 }))
    //             .Register(new FileSystemFileValueProcessor("OUT", "Output", Path.Combine(Environment.CurrentDirectory, "InputFiles"))),
    //         // Resolver = new SimpleDependencyResolver()
    //         //     .Register(dataAccess),
    //         TraceProcessDefinition = traceReporter.TraceProcessDefinition,
    //     };
    //     traceReporter.Initialize(structure);

    //     var res = await processRunner.ExecuteAsync(args, executionOptions);
    //     // res.OpenActualExecutionPlan();
    // }
    // static string? ConsoleAsk(string question)
    // {
    //     Console.WriteLine(question);
    //     return Console.ReadLine();
    // }

    //         /// <summary>
    //         /// Example 3: Import and export files using a config file to setup connectors
    //         /// </summary>
    //         /// <param name="args"></param>
    //         /// <returns></returns>
    //         static async Task ImportAndCreateFileWithConfigAsync(string[] args)
    //         {
    //             var processRunner = StreamProcessRunner.Create<string[]>(TestImport2.Import);
    //             var dataAccess = new DataAccess.TestDbContext();
    //             await dataAccess.Database.EnsureCreatedAsync();
    //             var executionOptions = new ExecutionOptions<string[]>
    //             {
    //                 Connectors = CreateConfigurationFileValueConnectorParser()
    //                     .GetConnectors(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "connectorsLocalConfig.json"))),
    //                 Resolver = new SimpleDependencyResolver()
    //                     .Register(dataAccess)
    //             };
    //             var res = await processRunner.ExecuteAsync(args, executionOptions);
    //         }
    //     }
}

public static class ApplicationConfigurationBuilder
{
    public static IConfigurationRoot GetConfiguration(string[]? args = null) => new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", true, true)
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true, true)
        .AddKeyPerFile("appsettings", true, true)
        .AddEnvironmentVariables()
        .AddEnvironmentVariables("SAMPLEETLNET_")
        .AddUserSecrets(typeof(Program).Assembly)
        .AddCommandLine(args ?? [], new Dictionary<string, string>
        {
            ["-cnx"] = "ConnectionStrings:SqlServer",
            ["--connectionString"] = "ConnectionStrings:SqlServer"
        })
        .Build();
}