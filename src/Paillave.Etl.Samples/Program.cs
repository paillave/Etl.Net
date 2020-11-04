using System;
// using Paillave.Etl.FromConfigurationConnectors;
using Paillave.Etl.FileSystem;
using Paillave.Etl.Connector;
using Paillave.Etl.Core;

namespace Paillave.Etl.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var executionOptions = new ExecutionOptions<string[]>
            {
                Connectors = new FileValueConnectors()
                    .Register(new FileSystemFileValueProvider("PTF", "Portfolios", "/home/stephane/Documents/Sources/PMS/src/Etl/Paillave.Etl.Samples/InputFiles", "*.Portfolios.csv"))
                    .Register(new FileSystemFileValueProvider("POS", "Positions", "/home/stephane/Documents/Sources/PMS/src/Etl/Paillave.Etl.Samples/InputFiles", "*.Positions.csv"))
                    .Register(new FileSystemFileValueProcessor("OUT", "Result", "/home/stephane/Documents/Sources/PMS/src/Etl/Paillave.Etl.Samples/InputFiles")),
                Resolver = new SimpleDependencyResolver()
                    .Register(new DataAccess.TestDbContext())
            };

            var processRunner = StreamProcessRunner.Create<string[]>(TestImport.Import);
            processRunner.ExecuteAsync(args, executionOptions);
        }
    }
}
