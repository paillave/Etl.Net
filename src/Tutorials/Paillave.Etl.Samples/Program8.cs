using System;
using System.Threading.Tasks;
using Paillave.Etl.Core;
using Paillave.Etl.Http;

namespace Paillave.Etl.Samples
{
    class Program8
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Debug 1");

            var processRunner = StreamProcessRunner.Create<string[]>(Import);
            var myHttpFactory = new SimpleHttpClientFactory();
            var myHttpClient = new HttpClient();
            myHttpFactory.RegisterNamedClient("http://127.0.0.1:80/anything", myHttpClient);
            myHttpFactory.RegisterNamedClient("http://127.0.0.1:80/ip", myHttpClient);
            var executionOptions = new ExecutionOptions<string[]>
            {
                Resolver = new SimpleDependencyResolver().Register<IHttpClientFactory>(
                    myHttpFactory
                ),
            };
            var res = await processRunner.ExecuteAsync(args, executionOptions);

            Console.WriteLine("Debug 2");
        }

        // HttpFileValueProvider(string code, string url, string method, string body)

        private static void DoOneTest(
            ISingleStream<string[]> contextStream,
            string testName,
            string method = "Get",
            string url = "http://127.0.0.1:80/ip",
            string body = null
        )
        {
            var fileStream = contextStream
                .CrossApply(
                    "Get Files",
                    new HttpFileValueProvider(
                        code: "codeTest",
                        url: url,
                        method: method,
                        body: body
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
                            Console.WriteLine($"result ({testName}): {contentString}");
                        }
                    }
                );
        }

        public static void Import(ISingleStream<string[]> contextStream)
        {
            // public HttpFileValueProvider(string code, string url, string method, string body)

            DoOneTest(contextStream, "test get", "Get");
            DoOneTest(
                contextStream,
                "test Post",
                "Post",
                "http://127.0.0.1:80/anything",
                "exemple body"
            );
        }
    }

    public class SimpleHttpClientFactory : IHttpClientFactory
    {
        private readonly IDictionary<string, HttpClient> _clients =
            new Dictionary<string, HttpClient>();

        public void RegisterNamedClient(string name, HttpClient client)
        {
            _clients[name] = client;
        }

        public HttpClient CreateClient(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                // Fallback behavior
                return _clients.Values.FirstOrDefault() ?? new HttpClient();
            }

            if (_clients.TryGetValue(name, out var client))
                return client;

            throw new ArgumentException($"No client registered with name {name}");
        }
    }
}
