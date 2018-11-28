using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Paillave.Etl.Reactive.Operators;
using System.IO;
using System.Diagnostics;
using System;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.Debugger.Extensions
{
    public static class StreamProcessDebugger
    {
        public static async Task<ExecutionStatus> OpenInDebugger<TConfig>(this Action<ISingleStream<TConfig>> jobDefinition, TConfig config)
        {
            var runner = StreamProcessRunner.Create<TConfig>(jobDefinition, "Debugged Job");
            return await runner.OpenInDebugger(config);
        }
        public static async Task<ExecutionStatus> OpenInDebugger<TConfig>(this StreamProcessRunner<TConfig> runner, TConfig config)
        {
            var etlTraceEventPusher = new EtlTraceEventPusher<TConfig>(runner, config);
            var webHost = WebHost.CreateDefaultBuilder(new string[] { }).ConfigureServices(services =>
                {
                    services.AddTransient<IEtlTraceEventPusher>(i => etlTraceEventPusher);
                }).UseStartup<Startup>().Build();
            webHost.Run();
            await etlTraceEventPusher.ResultTask.ContinueWith(i =>
            {
                webHost.StopAsync().Wait();
            });
            System.Diagnostics.Process.Start("http://localhost:5000");
            return await etlTraceEventPusher.ResultTask;
        }
    }
}