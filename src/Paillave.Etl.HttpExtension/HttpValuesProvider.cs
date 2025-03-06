// using System;
// using System.IO;
// using System.Text;
// using System.Threading;
// using Paillave.Etl.Core;


// namespace Paillave.Etl.HttpExtension;
// {
//     public class HttpValuesProviderArgs<TOut>
//     {
//         public Encoding Encoding { get; set; } = null;
//         public Func<string, TOut> GetResult { get; set; }
//         public bool UseStreamCopy { get; set; } = true;
//     }

//     public class HttpValuesProvider<TOut> : ValuesProviderBase<IFileValue, TOut>
//     {
//         private readonly HttpValuesProviderArgs<TOut> _args;

//         public HttpValuesProvider(HttpValuesProviderArgs<TOut> args)
//         {
//             this._args = args;
//         }

//         public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

//         public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

//         public override void PushValues(
//             IFileValue input,
//             Action<TOut> push,
//             CancellationToken cancellationToken,
//             IExecutionContext context
//         )
//         {
//             using var stream = input.Get(_args.UseStreamCopy);
//             using var sr =
//                 _args.Encoding == null
//                     ? new StreamReader(stream, true)
//                     : new StreamReader(stream, _args.Encoding);
//             while (!sr.EndOfStream)
//             {
//                 if (cancellationToken.IsCancellationRequested)
//                     break;
//                 push(_args.GetResult(sr.ReadLine()));
//             }
//         }
//     }
// }
