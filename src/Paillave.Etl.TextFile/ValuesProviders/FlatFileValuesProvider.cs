using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Paillave.Etl.TextFile.Core;

namespace Paillave.Etl.TextFile.ValuesProviders
{
    public class NameMappingFileValuesProviderArgs<TIn, TParsed, TOut> where TParsed : new()
    {
        public FileDefinition<TParsed> Mapping { get; set; }
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
            if (_args.Mapping.ReliesOnHeader)
            {
                var lineParserS = src
                    .Skip(_args.Mapping.FirstLinesToIgnore)
                    .Take(1)
                    .Map(_args.Mapping.WithHeaderLine)
                    .Map(i => i.GetSerializer());
                var ret = src
                    .Skip(1 + _args.Mapping.FirstLinesToIgnore)
                    .Filter(i => !string.IsNullOrWhiteSpace(i))
                    .CombineWithLatest(lineParserS, (txt, parser) => parser.Deserialize(txt))
                    .Map(i => _args.ResultSelector(input, i));
                return new DeferedWrapperPushObservable<TOut>(ret, src.Start);
            }
            else
            {
                var serializer = _args.Mapping.GetSerializer();
                var ret = src
                    .Skip(_args.Mapping.FirstLinesToIgnore)
                    .Filter(i => !string.IsNullOrWhiteSpace(i))
                    .Map(serializer.Deserialize)
                    .Map(i => _args.ResultSelector(input, i));
                return new DeferedWrapperPushObservable<TOut>(ret, src.Start);
            }
        }
    }
}
