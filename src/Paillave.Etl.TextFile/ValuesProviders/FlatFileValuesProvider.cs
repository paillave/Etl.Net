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
            var src = new PushSubject<string>();
            Exception exception = null;
            if (_args.Mapping.HasColumnHeader)
            {
                var numberedSrc = src
                    .Map((txt, idx) => new { txt, idx });
                var lineParserS = numberedSrc
                    .Skip(_args.Mapping.FirstLinesToIgnore)
                    .Take(1)
                    .Map(i => _args.Mapping.GetSerializer(i.txt))
                    .CompletesOnException(i => exception = i);
                numberedSrc
                    .Skip(1 + _args.Mapping.FirstLinesToIgnore)
                    .Filter(i => !string.IsNullOrWhiteSpace(i.txt))
                    .CombineWithLatest(lineParserS, (line, parser) =>
                    {
                        try
                        {
                            return parser.Deserialize(line.txt);
                        }
                        catch (Exception ex)
                        {
                            throw new FlatFileLineDeserializeException(line.idx, ex);
                        }
                    })
                    .CompletesOnException(i => exception = i)
                    .Map(i => _args.ResultSelector(input, i))
                    .Do(push);
            }
            else
            {
                var serializer = _args.Mapping.GetSerializer();
                var numberedSrc = src
                    .Map((txt, idx) => new { txt, idx });
                numberedSrc
                    .Skip(_args.Mapping.FirstLinesToIgnore)
                    .Filter(i => !string.IsNullOrWhiteSpace(i.txt))
                    .Map(i =>
                    {
                        try
                        {
                            return serializer.Deserialize(i.txt);
                        }
                        catch (Exception ex)
                        {
                            throw new FlatFileLineDeserializeException(i.idx, ex);
                        }
                    })
                    .Map(i => _args.ResultSelector(input, i))
                    .Do(push)
                    .CompletesOnException(i => exception = i);
            }

            using (var sr = new StreamReader(_args.DataStreamSelector(input)))
                while (!sr.EndOfStream)
                    src.PushValue(sr.ReadLine());

            if (exception != null)
                throw exception;
            //src.PushException(exception);
            src.Complete();
        }
    }
}
