using Paillave.Etl.Core;
using Paillave.Etl.Helpers;
using Paillave.Etl.StreamNodes;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Paillave.Etl.ValuesProviders
{
    public class NameMappingFileValuesProviderArgs<TIn,TParsed, TOut> where TParsed : new()
    {
        public ColumnNameFlatFileDescriptor<TParsed> Mapping { get; set; }
        public Func<TIn, TParsed, TOut> ResultSelector { get; set; }
        public Func<TIn, Stream> DataStreamSelector { get; set; }
        public bool NoParallelisation { get; set; } = false;
    }
    public class NameMappingFileValuesProvider<TIn, TParsed, TOut> : ValuesProviderBase<TIn, TOut> where TParsed : new()
    {
        private NameMappingFileValuesProviderArgs<TIn, TParsed, TOut> _args;
        public NameMappingFileValuesProvider(NameMappingFileValuesProviderArgs<TIn, TParsed, TOut> args) : base(args.NoParallelisation)
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
            var lineParserS = splittedLineS
                .Skip(_args.Mapping.LinesToIgnore)
                .Take(1)
                .Map(_args.Mapping.ColumnNameMappingConfiguration.LineParser);
            var dataLineS = splittedLineS
                .Skip(1 + _args.Mapping.LinesToIgnore)
                .Filter(i => i.Count > 0);
            var ret = dataLineS.CombineWithLatest(lineParserS, (dataLine, lineParser) => _args.ResultSelector(input, lineParser(dataLine)));
            return new DeferedWrapperPushObservable<TOut>(ret, src.Start);
        }
    }
}
