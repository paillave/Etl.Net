// using System.Net.Http;
// using Paillave.Etl.Core;
// using Paillave.Etl.Reactive.Operators;

// namespace Paillave.Etl.Http;

// public class HttpRestStreamNode
//     : StreamNodeBase<HttpResponseMessage, IStream<HttpResponseMessage>, HttpArgs>
// {
//     public HttpRestStreamNode(string name, HttpArgs args)
//         : base(name, args) { }

//     public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
//     public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

//     protected override IStream<HttpResponseMessage> CreateOutputStream(HttpArgs args)
//     {
//         var outputObservable = args.Stream.Observable.Map(httpCallArgs =>
//         {
//             var httpClient = IHttpConnectionInfoEx.CreateHttpClient(
//                 httpCallArgs.ConnectionParameters,
//                 httpCallArgs.AdapterParameters
//             );

//             var response = HttpHelpers
//                 .GetResponse(
//                     httpCallArgs.ConnectionParameters,
//                     httpCallArgs.AdapterParameters,
//                     httpClient
//                 )
//                 .Result;

//             return response;
//         });

//         return CreateUnsortedStream(outputObservable);
//     }
// }

// public class HttpArgs
// {
//     public required IStream<HttpCallArgs> Stream { get; set; }
// }
