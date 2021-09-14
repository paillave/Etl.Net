using System.Threading.Tasks;
using Paillave.Etl.Core;

namespace BlogTutorial
{
    class SomeExternalValue
    {
        public string AStringValue { get; set; }
        public int AnIntValue { get; set; }
    }
    class Program3
    {
        static async Task Main3(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string>(DefineProcess);
            var executionOptions = new ExecutionOptions<string>
            {
                Resolver = new SimpleDependencyResolver()
                    .Register(new SomeExternalValue
                    {
                        AStringValue = "injected string",
                        AnIntValue = 658
                    })
            };
            var res = await processRunner.ExecuteAsync("transmitted parameter", executionOptions);
        }
        private static void DefineProcess(ISingleStream<string> contextStream)
        {
            contextStream
                .ResolveAndSelect("get some values", o => o
                    .Resolve<SomeExternalValue>()
                    .Select((context, injected) => $"{context}-{injected.AStringValue}:{injected.AnIntValue}"));
        }
    }
}
