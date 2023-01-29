using Paillave.Etl.Core;
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
        public bool UseStreamCopy { get; set; } = true;
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

        public override void PushValues(IFileValue input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
        {
            using var stream = input.Get(_args.UseStreamCopy);
            context.AddUnderlyingDisposables(stream);
            using var sr = _args.Encoding == null ? new StreamReader(stream, true) : new StreamReader(stream, _args.Encoding);
            while (!sr.EndOfStream)
            {
                if (cancellationToken.IsCancellationRequested) break;
                push(_args.GetResult(sr.ReadLine()));
            }
        }
    }
}
