using Newtonsoft.Json;
using Paillave.Etl.Core;
using Paillave.Etl.Http;
using Paillave.Etl.JsonFile;

namespace Paillave.Etl.Samples
{
    public class TestRoot
    {
        [JsonProperty("headers")]
        public TestHeaders? Headers { get; set; }

        public override string ToString() => $"{{ headers: {Headers?.Host} }}";
    }

    public class TestHeaders
    {
        [JsonProperty("Host")]
        public string? Host { get; set; }
    }

    class Program9
    {
        static async Task Main(string[] args)
        {
            var httpAdapterConnectionParameters = new HttpAdapterConnectionParameters
            {
                Url = "http://127.0.0.1:80/headers",
            };
            var httpAdapterProviderParameters = new HttpAdapterProviderParameters
            {
                Method = HttpMethodCustomEnum.Get,
            };
            var httpProcessorConnectionParameters = new HttpAdapterConnectionParameters
            {
                Url = "http://127.0.0.1:80/anything",
            };
            var httpAdapterProcessorsParameters = new HttpAdapterProcessorParameters
            {
                Method = HttpMethodCustomEnum.Post,
                UserResponseAsOutput = true,
            };
            var httpAdapterProcessorsParameters2 = new HttpAdapterProcessorParameters
            {
                Method = HttpMethodCustomEnum.Post,
                UserResponseAsOutput = false,
            };

            var connectors = new FileValueConnectors();
            connectors.Register(
                new HttpFileValueProvider(
                    "MyHttpSourceForThePurposeX",
                    "Input",
                    "HttpConnection",
                    httpAdapterConnectionParameters,
                    httpAdapterProviderParameters
                )
            );
            connectors.Register(
                new HttpFileValueProcessor(
                    "MyHttpSourceForThePurposeY",
                    "Output",
                    "HttpConnection2",
                    httpProcessorConnectionParameters,
                    httpAdapterProcessorsParameters
                )
            );
            connectors.Register(
                new HttpFileValueProcessor(
                    "MyHttpSourceForThePurposeY_UserResponseAsOutput",
                    "Output",
                    "HttpConnection2",
                    httpProcessorConnectionParameters,
                    httpAdapterProcessorsParameters2
                )
            );

            var executionOptions = new ExecutionOptions<string[]> { Connectors = connectors };

            var processRunner = StreamProcessRunner.Create<string[]>(Import);

            // var executionOptions = new ExecutionOptions<string[]>
            // {
            //     Resolver = new SimpleDependencyResolver().Register<HttpClientFactory>(
            //         httpClientFactory
            //     ),
            // };
            var res = await processRunner.ExecuteAsync(args, executionOptions);
        }

        public static void Import(ISingleStream<string[]> contextStream)
        {
            contextStream
                .FromConnector("get from http", "MyHttpSourceForThePurposeX")
                .Do(
                    "print to console",
                    i =>
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            i.GetContent().CopyTo(memoryStream);
                            byte[] contentBytes = memoryStream.ToArray();
                            string contentString = System.Text.Encoding.UTF8.GetString(
                                contentBytes
                            );
                            Console.WriteLine($"result ({contentString}");
                        }
                    }
                )
                .ParseJson<TestRoot>("parseJson")
                .Do(
                    "print to console (after ParseJson)",
                    i => Console.WriteLine($"\nafter ParseJson : {i.ToString()}")
                )
                .SerializeToJsonFileValue<TestRoot>("serializeJson")
                .ToConnector("post to http", "MyHttpSourceForThePurposeY")
                .Do(
                    "result of connectors",
                    i => Console.WriteLine($"\nafter connector : {i.ToString()}")
                )
                .ToConnector("post to http", "MyHttpSourceForThePurposeY_UserResponseAsOutput")
                .Do(
                    "result of connectors",
                    i =>
                        Console.WriteLine(
                            $"\nafter connector (UseResponseAsOutput) : {i.ToString()}"
                        )
                );

            // .Do(
            //     "print to console",
            //     i =>
            //     {
            //         using (var memoryStream = new MemoryStream())
            //         {
            //             i.GetContent().CopyTo(memoryStream);
            //             byte[] contentBytes = memoryStream.ToArray();
            //             string contentString = System.Text.Encoding.UTF8.GetString(
            //                 contentBytes
            //             );
            //             Console.WriteLine($"result ({contentString}");
            //         }
            //     }
            // );

            // contextStream
            //     .Select(
            //         "create required stream item type",
            //         i => new HttpCallArgs
            //         {
            //             ConnectionParameters = new HttpAdapterConnectionParameters
            //             {
            //                 Url = "http://127.0.0.1:80/headers/",
            //             },
            //             AdapterParameters = new HttpAdapterParametersBase
            //             {
            //                 Method = HttpMethodCustomEnum.GET,
            //             },
            //         }
            //     )
            //     .HttpRest("http rest operator test")
            //     .Do(
            //         "print to console",
            //         i =>
            //         {
            //             using (var memoryStream = new MemoryStream())
            //             {
            //                 string contentString = System.Text.Encoding.UTF8.GetString(
            //                     i.Content.ReadAsByteArrayAsync().Result
            //                 );
            //                 Console.WriteLine($"result : {contentString}");
            //             }
            //         }
            //     );
        }
    }
}
