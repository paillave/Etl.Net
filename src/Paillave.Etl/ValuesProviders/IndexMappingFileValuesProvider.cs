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
    public class IndexMappingFileValuesProviderArgs<TIn,TParsed, TOut> where TParsed : new()
    {
        public ColumnIndexFlatFileDescriptor<TParsed> Mapping { get; set; }
        public Func<TIn, TParsed, TOut> ResultSelector { get; set; }
        public Func<TIn, Stream> DataStreamSelector { get; set; }
        public bool NoParallelisation { get; set; } = false;
    }
    public class IndexMappingFileValuesProvider<TIn, TParsed, TOut> : ValuesProviderBase<TIn, TOut> where TParsed : new()
    {
        private IndexMappingFileValuesProviderArgs<TIn, TParsed, TOut> _args;
        public IndexMappingFileValuesProvider(IndexMappingFileValuesProviderArgs<TIn, TParsed, TOut> args) : base(args.NoParallelisation)
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
            var splittedLineS = src.Map(_args.Mapping.LineSplitter);

            var dataLineS = splittedLineS.Skip(_args.Mapping.LinesToIgnore).Filter(i => i.Count > 0);
            var inputLineParser = _args.Mapping.ColumnIndexMappingConfiguration.LineParser();
            var ret = dataLineS.Map(dataLine => _args.ResultSelector(input, inputLineParser(dataLine)));
            return new DeferedWrapperPushObservable<TOut>(ret, src.Start);
        }
    }
}
