using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.Ftp;
using Paillave.Pdf;

namespace Paillave.Etl.Samples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var processRunner = StreamProcessRunner.Create<string[]>(Import);
            var res = await processRunner.ExecuteAsync(args);
        }
        public static void Import(ISingleStream<string[]> contextStream)
        {
            var portfolioFileStream = contextStream.CrossApply("Get Files", new FtpFileValueProvider("SRC", "Solar exports", "files from ftp", new FtpAdapterConnectionParameters{
                Server="localhost",
                Login="stephane",
                Password="xxxxxxxx"
            }, new FtpAdapterProviderParameters{

            }))
        .Do("print files to console", i => Console.WriteLine(i.Name));
        }
    }
}
