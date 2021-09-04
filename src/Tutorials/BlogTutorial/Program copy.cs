using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace BlogTutorial
{
    class Program2
    {
        static async Task Main2(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            var executionOptions = new ExecutionOptions<string>
            {
                TraceProcessDefinition = DefineTraceProcess
            };
            var res = await processRunner.ExecuteAsync(args[0], executionOptions);
        }
        private static void DefineTraceProcess(IStream<TraceEvent> traceStream, ISingleStream<string> contentStream)
        {
            // TODO: define how to process traces here
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            // TODO: define the process here
        }
    }
}
