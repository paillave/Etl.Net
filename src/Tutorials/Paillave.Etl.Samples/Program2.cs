using System;
// using Paillave.Etl.FromConfigurationConnectors;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Core;
using Paillave.Etl.ExecutionToolkit;
using System.Threading.Tasks;
using System.IO;
using Paillave.Etl.FromConfigurationConnectors;
using Paillave.Etl.Ftp;
using Paillave.Etl.Sftp;
using Paillave.Etl.Mail;
using Paillave.Etl.Zip;

namespace Paillave.Etl.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await ImportAndCreateFileAsync(args);
            // await ImportAndCreateFileWithConfigAsync(args);
        }
        private static ConfigurationFileValueConnectorParser CreateConfigurationFileValueConnectorParser() => new ConfigurationFileValueConnectorParser(
                new FileSystemProviderProcessorAdapter(),
                new FtpProviderProcessorAdapter(),
                new SftpProviderProcessorAdapter(),
                new MailProviderProcessorAdapter());
        public static void CreateConnectorConfigurationFileSchema()
            => File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "connectorsConfigSchema.json"), CreateConfigurationFileValueConnectorParser().GetConnectorsSchemaJson());

        /// <summary>
        /// Example 1: simple importation of a set of files
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task SimplyImportAsync(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string[]>(TestImport.Import);
            // processRunner.GetDefinitionStructure().OpenEstimatedExecutionPlan();
            var output = await processRunner.ExecuteAsync(args, new ExecutionOptions<string[]>
            {
                Connectors = new FileValueConnectors()
                    .Register(new FileSystemFileValueProvider("PTF", "Portfolios", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Portfolios.csv"))
                    .Register(new FileSystemFileValueProvider("POS", "Positions", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Positions.csv")),
                Resolver = new SimpleDependencyResolver()
                    .Register(new DataAccess.TestDbContext()),
            });
        }

        /// <summary>
        /// Example 2: Import and export files showing the estimated execution plan, the real time evolution of the process, and the actual execution plan
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task ImportAndCreateFileAsync(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string[]>(TestImport2.Import);
            var structure = processRunner.GetDefinitionStructure();
            structure.OpenEstimatedExecutionPlan();

            ITraceReporter traceReporter = new AdvancedConsoleExecutionDisplay();
            var dataAccess= new DataAccess.TestDbContext();
            await dataAccess.Database.EnsureCreatedAsync();
            var executionOptions = new ExecutionOptions<string[]>
            {
                Connectors = new FileValueConnectors()
                    .Register(new FileSystemFileValueProvider("PTF", "Portfolios", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Portfolios.csv"))
                    .Register(new FileSystemFileValueProvider("POS", "Positions", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Positions.csv"))
                    .Register(new FileSystemFileValueProcessor("OUT", "Result", Path.Combine(Environment.CurrentDirectory, "InputFiles"))),
                Resolver = new SimpleDependencyResolver()
                    .Register(dataAccess),
                TraceProcessDefinition = traceReporter.TraceProcessDefinition,
            };
            traceReporter.Initialize(structure);

            var res = await processRunner.ExecuteAsync(args, executionOptions);
            res.OpenActualExecutionPlan();
        }

        /// <summary>
        /// Example 3: Import and export files using a config file to setup connectors
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task ImportAndCreateFileWithConfigAsync(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string[]>(TestImport2.Import);
            var dataAccess= new DataAccess.TestDbContext();
            await dataAccess.Database.EnsureCreatedAsync();
            var executionOptions = new ExecutionOptions<string[]>
            {
                Connectors = CreateConfigurationFileValueConnectorParser()
                    .GetConnectors(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "connectorsLocalConfig.json"))),
                Resolver = new SimpleDependencyResolver()
                    .Register(dataAccess)
            };
            var res = await processRunner.ExecuteAsync(args, executionOptions);
        }
    }
}
