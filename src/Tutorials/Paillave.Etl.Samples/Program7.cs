// using System;
// using System.Threading.Tasks;
// using Paillave.Etl.Core;
// using Paillave.Etl.Http;

// namespace Paillave.Etl.Samples
// {
//     class Program7
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
//             HttpMethodCustomEnum method = HttpMethodCustomEnum.GET,
//             string url = "http://127.0.0.1:80/ip"
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
//             // DoOneTest(contextStream, "no auth", "None", new List<string>());

//             // DoOneTest(
//             //     contextStream,
//             //     "Bearer auth",
//             //     "Bearer",
//             //     new List<string>() { "AbCdEf123456" },
//             //     "/bearer"
//             // );

//             // DoOneTest(
//             //     contextStream,
//             //     "Basic auth (OK)",
//             //     "Basic",
//             //     new List<string>() { "usr", "pwd" },
//             //     "/basic-auth/usr/pwd"
//             // );

//             DoOneTest(
//                 contextStream,
//                 "Basic auth (NOK)"
//             // "Basic",
//             // new List<string>() { "usr", "pwdWrong" },
//             );

//             // DoOneTest(
//             //     contextStream,
//             //     "Digest (usr/pwd endpoint)",
//             //     "Digest",
//             //     new List<string>() { "usr", "pwd" },
//             //     "/digest-auth/auth/usr/pwd"
//             // );

//             // DoOneTest(
//             //     contextStream,
//             //     "Digest (usr/pwd/MD5 endpoint)",
//             //     "Digest",
//             //     new List<string>() { "usr", "pwd", "auth", "MD5" },
//             //     "/digest-auth/auth/usr/pwd/MD5"
//             // );

//             // DoOneTest(
//             //     contextStream,
//             //     "ApiKey auth",
//             //     "ApiKey",
//             //     new List<string>() { "ApiKeyExemple" }
//             // );

//             // DoOneTest(
//             //     contextStream,
//             //     "CustomHeader auth",
//             //     "CustomHeader",
//             //     new List<string>() { "arg1", "val1", "arg2", "val2" }
//             // );
//         }
//     }
// }
