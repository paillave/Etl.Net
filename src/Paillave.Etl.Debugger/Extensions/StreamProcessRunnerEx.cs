using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Debugger.Extensions
{
    public static class StreamProcessRunnerEx
    {
        public static async Task<ExecutionStatus> OpenInDebugger<TConfig>(this StreamProcessRunner<TConfig> streamProcessRunner, TConfig config)
        {
            var etlTraceEventPusher = new EtlTraceEventPusher<TConfig>(streamProcessRunner, config);
            // cls.Start();
            var webHost = WebHost.CreateDefaultBuilder(new string[] { }).ConfigureServices(services =>
                {
                    services.AddTransient<IEtlTraceEventPusher>(i => etlTraceEventPusher);
                }).UseStartup<Startup>().Build();
            webHost.Run();

            await etlTraceEventPusher.ResultTask.ContinueWith(i =>
            {
                webHost.StopAsync();
            });

            return await etlTraceEventPusher.ResultTask;
        }
    }
}