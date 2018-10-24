using Paillave.Etl.Core;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Paillave.Etl.TextFile.Core;
using System.Threading.Tasks;

namespace Paillave.Etl.TextFile.ValuesProviders
{
    public class FlatFileValuesProviderArgs<TIn, TParsed, TOut>
    {
        public FlatFileDefinition<TParsed> Mapping { get; set; }
        public Func<TIn, TParsed, TOut> ResultSelector { get; set; }
        public Func<TIn, Stream> DataStreamSelector { get; set; }
    }
    public class FlatFileValuesProvider<TIn, TParsed, TOut>
    {
        private FlatFileValuesProviderArgs<TIn, TParsed, TOut> _args;
        public FlatFileValuesProvider(FlatFileValuesProviderArgs<TIn, TParsed, TOut> args)
        {
            _args = args;
        }
        public void PushValues(TIn input, Action<TOut> push)
        {
            var src = new DeferredPushObservable<string>(pushValue =>
            {
                using (var sr = new StreamReader(_args.DataStreamSelector(input)))
                    while (!sr.EndOfStream)
                        pushValue(sr.ReadLine());
            });
            IPushObservable<TOut> ret;
            if (_args.Mapping.HasColumnHeader)
            {
                var lineParserS = src
                    .Skip(_args.Mapping.FirstLinesToIgnore)
                    .Take(1)
                    .Map(_args.Mapping.GetSerializer);
                ret = src
                    .Skip(1 + _args.Mapping.FirstLinesToIgnore)
                    .Filter(i => !string.IsNullOrWhiteSpace(i))
                    .CombineWithLatest(lineParserS, (txt, parser) => parser.Deserialize(txt))
                    .Map(i => _args.ResultSelector(input, i))
                    .Do(push);
                new DeferredWrapperPushObservable<TOut>(ret, src.Start);
            }
            else
            {
                var serializer = _args.Mapping.GetSerializer();
                ret = src
                    .Skip(_args.Mapping.FirstLinesToIgnore)
                    .Filter(i => !string.IsNullOrWhiteSpace(i))
                    .Map(serializer.Deserialize)
                    .Map(i => _args.ResultSelector(input, i))
                    .Do(push);
                new DeferredWrapperPushObservable<TOut>(ret, src.Start);
            }
            var task = ret.ToTaskAsync();
            src.Start();
            task.Wait();
        }
    }
}
