using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.HttpExtension;

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

            var portfolioFileStream = contextStream
                .CrossApply(
                    "Get Files",
                    new HttpFileValueProvider(
                        "codeTest",
                        "nameTest",
                        "connectionNameTest",
                        "http://127.0.0.1:80/",
                        "Get",
                        "/ip",
                        "json",
                        null,
                        new List<string>(),
                        "None",
                        null
                    )
                )
                .Do(
                    "print to console",
                    i =>
                    {
                        // Convert the Stream content into a byte array
                        using (var memoryStream = new MemoryStream())
                        {
                            i.GetContent().CopyTo(memoryStream);
                            byte[] contentBytes = memoryStream.ToArray();
                            string contentString = System.Text.Encoding.UTF8.GetString(
                                contentBytes
                            ); // Convert byte array to string
                            Console.WriteLine($"result: {contentString}"); // Print the result as a string
                        }
                    }
                );

            Console.WriteLine("Debug 4");
        }
    }
}
