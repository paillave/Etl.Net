// using System;
// using System.Threading.Tasks;
// using Paillave.Etl.Core;
// using Paillave.Etl.Http;

// namespace Paillave.Etl.Samples
// {
//     class Program9
//     {
//         static async Task Main(string[] args)
//         {
//             var processRunner = StreamProcessRunner.Create<string[]>(Import);

//             // var executionOptions = new ExecutionOptions<string[]>
//             // {
//             //     Resolver = new SimpleDependencyResolver().Register<HttpClientFactory>(
//             //         httpClientFactory
//             //     ),
//             // };
//             var res = await processRunner.ExecuteAsync(
//                 args /*, executionOptions*/
//             );
//         }

//         private static void DoOneTest(
//             ISingleStream<string[]> contextStream,
//             string testName,
//             // string authType,
//             // List<string> authParams,
//             HttpMethods method = HttpMethods.GET,
//             string url = "http://127.0.0.1:80/"
//         )
//         {
//             var fileStream = contextStream
//                 .CrossApply(
//                     "Get Files",
//                     new HttpFileValueProvider(
//                         code: "codeTest",
//                         name: url,
//                         connectionName: url,
//                         connectionParameters: new HttpAdapterConnectionParameters { Url = url },
//                         providerParameters: new HttpAdapterProviderParameters { Method = method }
//                     )
//                 )
//                 .Do(
//                     "print to console",
//                     i =>
//                     {
//                         // Convert the Stream content into a byte array
//                         using (var memoryStream = new MemoryStream())
//                         {
//                             i.GetContent().CopyTo(memoryStream);
//                             byte[] contentBytes = memoryStream.ToArray();
//                             string contentString = System.Text.Encoding.UTF8.GetString(
//                                 contentBytes
//                             ); // Convert byte array to string
//                             Console.WriteLine($"result ({testName}): {contentString}");
//                         }
//                     }
//                 );
//         }

//         public static void Import(ISingleStream<string[]> contextStream)
//         {
//             // contextStream
//             //     .FromConnector("get from http", "MyHttpSourceForThePurposeX")
//             //     .ParseJson<MyTargetType>("parseJson")
//             //     .SerializeToJson("serializeJson")
//             //     .ToConnector("post to http", "MyHttpSourceForThePurposeY");

//             contextStream
//                 .Select(
//                     "create required stream item type",
//                     i => new HttpCallArgs
//                     {
//                         ConnectionParameters = new HttpAdapterConnectionParameters
//                         {
//                             Url = "http://127.0.0.1:80/headers/",
//                         },
//                         AdapterParameters = new HttpAdapterParametersBase
//                         {
//                             Method = HttpMethods.GET,
//                         },
//                     }
//                 )
//                 .HttpRest("http rest operator test")
//                 .Do(
//                     "print to console",
//                     i =>
//                     {
//                         using (var memoryStream = new MemoryStream())
//                         {
//                             string contentString = System.Text.Encoding.UTF8.GetString(
//                                 i.Content.ReadAsByteArrayAsync().Result
//                             );
//                             Console.WriteLine($"result : {contentString}");
//                         }
//                     }
//                 );
//         }
//     }
// }
