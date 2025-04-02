// using System.Net.Http;
// using Paillave.Etl.Core;

// namespace Paillave.Etl.Http;

// public static partial class HttpCallEx
// {
//     public static IStream<HttpResponseMessage> HttpRest(
//         this IStream<HttpCallArgs> stream,
//         string name
//     ) => new HttpRestStreamNode(name, new HttpArgs { Stream = stream }).Output;
// }
