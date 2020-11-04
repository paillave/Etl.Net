using System;
// using Paillave.Etl.FromConfigurationConnectors;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Connector;
using Paillave.Etl.Core;
using Paillave.Etl.ExecutionToolkit;
using System.Threading.Tasks;
using System.IO;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string[]>(TestImport2.Import);
            var structure = processRunner.GetDefinitionStructure();
            structure.OpenEstimatedExecutionPlan();

            ITraceReporter traceReporter = new AdvancedConsoleExecutionDisplay();
            var executionOptions = new ExecutionOptions<string[]>
            {
                Connectors = new FileValueConnectors()
                    .Register(new FileSystemFileValueProvider("PTF", "Portfolios", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Portfolios.csv"))
                    .Register(new FileSystemFileValueProvider("POS", "Positions", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Positions.csv"))
                    .Register(new FileSystemFileValueProcessor("OUT", "Result", Path.Combine(Environment.CurrentDirectory, "InputFiles"))),
                Resolver = new SimpleDependencyResolver()
                    .Register(new DataAccess.TestDbContext()),
                TraceProcessDefinition = (i, j) => i.Observable.Do(traceReporter.HandleTrace)
            };
            traceReporter.Initialize(structure);

            var res = await processRunner.ExecuteAsync(args, executionOptions);
            res.OpenActualExecutionPlan();
        }
        static async Task MainSimple(string[] args)
        {
            var executionOptions = new ExecutionOptions<string[]>
            {
                Connectors = new FileValueConnectors()
                    .Register(new FileSystemFileValueProvider("PTF", "Portfolios", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Portfolios.csv"))
                    .Register(new FileSystemFileValueProvider("POS", "Positions", Path.Combine(Environment.CurrentDirectory, "InputFiles"), "*.Positions.csv")),
                Resolver = new SimpleDependencyResolver()
                    .Register(new DataAccess.TestDbContext()),
            };
            var processRunner = StreamProcessRunner.Create<string[]>(TestImport.Import);
            await processRunner.ExecuteAsync(args, executionOptions);
        }
    }
}
