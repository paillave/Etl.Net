using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Paillave.Etl.TextFile
{
    public class TextFileValuesProviderArgs<TOut>
    {
        public Encoding Encoding { get; set; } = null;
        public Func<string, TOut> GetResult { get; set; }
    }
    public class TextFileValuesProvider<TOut> : ValuesProviderBase<Stream, TOut>
    {
        private readonly TextFileValuesProviderArgs<TOut> _args;

        public TextFileValuesProvider(TextFileValuesProviderArgs<TOut> args)
        {
            this._args = args;
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public override void PushValues(Stream input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var sr = _args.Encoding == null ? new StreamReader(input, true) : new StreamReader(input, _args.Encoding);
            using (sr)
                while (!sr.EndOfStream)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    push(_args.GetResult(sr.ReadLine()));
                }
        }
    }
}
