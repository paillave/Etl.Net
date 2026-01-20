using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;

namespace BlogTutorial;

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
            Services = new ServiceCollection().AddSingleton(new SomeExternalValue
            {
                AStringValue = "injected string",
                AnIntValue = 658
            }).BuildServiceProvider()
        };
        var res = await processRunner.ExecuteAsync("transmitted parameter", executionOptions);
    }
    private static void DefineProcess(ISingleStream<string> contextStream)
    {
        contextStream
            .SelectResolved("get some values", (context, services) => $"{context}-{services.GetRequiredService<SomeExternalValue>().AStringValue}:{services.GetRequiredService<SomeExternalValue>().AnIntValue}");
    }
}
