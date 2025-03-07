// using System;
// using System.Threading.Tasks;
// using Paillave.Etl.Core;
// using Paillave.Etl.HttpExtension;

// namespace Paillave.Etl.Samples
// {
//     class Program7
//     {
//         static async Task Main(string[] args)
//         {
//             Console.WriteLine("Debug 1");

//             var processRunner = StreamProcessRunner.Create<string[]>(Import);
//             var res = await processRunner.ExecuteAsync(args);

//             Console.WriteLine("Debug 2");
//         }

//         private static void DoOneTest(
//             ISingleStream<string[]> contextStream,
//             string testName,
//             string authType,
//             List<string> authParams,
//             string slug = "/headers",
//             string method = "Get"
//         )
//         {
//             var fileStream = contextStream
//                 .CrossApply(
//                     "Get Files",
//                     new HttpFileValueProvider(
//                         "codeTest",
//                         "nameTest",
//                         "connectionNameTest",
//                         "http://127.0.0.1:80/",
//                         method,
//                         slug,
//                         "json",
//                         null,
//                         authParams,
//                         authType,
//                         null
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
//             // public HttpFileValueProvider(string code, string url, string method, string body)

//             Console.WriteLine("Debug 3");

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

//             // DoOneTest(
//             //     contextStream,
//             //     "Basic auth (NOK)",
//             //     "Basic",
//             //     new List<string>() { "usr", "pwdWrong" },
//             //     "/basic-auth/usr/pwd"
//             // );

//             DoOneTest(
//                 contextStream,
//                 "Digest (usr/pwd endpoint)",
//                 "Digest",
//                 new List<string>() { "usr", "pwd" },
//                 "/digest-auth/auth/usr/pwd"
//             );

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

//             Console.WriteLine("Debug 4");
//         }
//     }
// }
