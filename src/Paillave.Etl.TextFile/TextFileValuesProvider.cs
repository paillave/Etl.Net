using Paillave.Etl.Core;
using Paillave.Etl.ValuesProviders;
using System;
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
    public class TextFileValuesProvider<TOut> : ValuesProviderBase<IFileValue, TOut>
    {
        private readonly TextFileValuesProviderArgs<TOut> _args;

        public TextFileValuesProvider(TextFileValuesProviderArgs<TOut> args)
        {
            this._args = args;
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        public override void PushValues(IFileValue input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var sr = _args.Encoding == null ? new StreamReader(input.GetContent(), true) : new StreamReader(input.GetContent(), _args.Encoding);
            using (sr)
                while (!sr.EndOfStream)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    push(_args.GetResult(sr.ReadLine()));
                }
        }
    }
}
