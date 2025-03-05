using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.Ftp;
using Paillave.Etl.Samples.DataAccess;
using Paillave.Etl.Http;

namespace Paillave.Etl.Samples
{
    class Program7
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Debug 1");

            var processRunner = StreamProcessRunner.Create<string[]>(Import);
            var res = await processRunner.ExecuteAsync(args);

            Console.WriteLine("Debug 2");
        }
        public static void Import(ISingleStream<string[]> contextStream)
        {
            // public HttpFileValueProvider(string code, string url, string method, string body)

            Console.WriteLine("Debug 3");
    
            var portfolioFileStream = contextStream.CrossApply("Get Files", new HttpFileValueProvider("code", "https://127.0.01:80", "Get", ""))
            .Do("print to console", i => Console.WriteLine(i.Name));

            Console.WriteLine("Debug 4");
        }
    }
}
