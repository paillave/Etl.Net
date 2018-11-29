using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Paillave.Etl.Debugger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new Coordinator.Inspector().GetEtlList(@"C:\Users\sroyer\Source\Repos\Etl.Net\src\Samples\ComplexQuickstart\bin\Debug\netcoreapp2.1\ComplexQuickstart.dll");
            // // var cls = new EtlTraceEventPusher();
            // WebHost.CreateDefaultBuilder(args).ConfigureServices(services =>
            // {
            //     // services.AddTransient(i => cls);
            // }).UseStartup<Startup>().Build().Run();
        }
    }
}
