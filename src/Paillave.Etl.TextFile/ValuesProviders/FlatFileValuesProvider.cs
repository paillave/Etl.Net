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
            using (var sr = new StreamReader(_args.DataStreamSelector(input)))
            {
                if (_args.Mapping.HasColumnHeader)
                {
                    int index = 0;
                    LineSerializer<TParsed> lineSerializer = null;
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        if (index == _args.Mapping.FirstLinesToIgnore)
                        {
                            lineSerializer = _args.Mapping.GetSerializer(line);
                        }
                        else if (index > _args.Mapping.FirstLinesToIgnore && !string.IsNullOrWhiteSpace(line))
                        {
                            TParsed parsed;
                            try
                            {
                                parsed = lineSerializer.Deserialize(line);
                            }
                            catch (Exception ex)
                            {
                                throw new FlatFileLineDeserializeException(index, ex);
                            }
                            push(_args.ResultSelector(input, parsed));
                        }
                        index++;
                    }
                }
                else
                {
                    int index = 0;
                    LineSerializer<TParsed> lineSerializer = _args.Mapping.GetSerializer();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();

                        if (index >= _args.Mapping.FirstLinesToIgnore && !string.IsNullOrWhiteSpace(line))
                        {
                            TParsed parsed;
                            try
                            {
                                parsed = lineSerializer.Deserialize(line);
                            }
                            catch (Exception ex)
                            {
                                throw new FlatFileLineDeserializeException(index, ex);
                            }
                            push(_args.ResultSelector(input, parsed));
                        }
                        index++;
                    }
                }
            }
        }
    }
}
