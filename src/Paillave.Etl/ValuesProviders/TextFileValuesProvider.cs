using Paillave.Etl.Core;
using Paillave.Etl.Helpers;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Paillave.Etl.ValuesProviders
{
    public class TextFileValuesProviderArgs<TIn, TOut>
    {
        public Func<TIn, string, TOut> ResultSelector { get; set; }
        public Func<TIn, Stream> DataStreamSelector { get; set; }
        public bool NoParallelisation { get; set; } = false;
    }
    public class TextFileValuesProvider<TIn, TOut> : ValuesProviderBase<TIn, TOut>
    {
        private TextFileValuesProviderArgs<TIn, TOut> _args;
        public TextFileValuesProvider(TextFileValuesProviderArgs<TIn, TOut> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        public override IDeferedPushObservable<TOut> PushValues(TIn input)
        {
            var src = new DeferedPushObservable<string>(pushValue =>
            {
                WaitOne();
                using (var sr = new StreamReader(_args.DataStreamSelector(input)))
                    while (!sr.EndOfStream)
                        pushValue(sr.ReadLine());
                Release();
            });
            var ret = src.Map(dataLine => _args.ResultSelector(input, dataLine));
            return new DeferedWrapperPushObservable<TOut>(ret, src.Start);
        }
    }
}
