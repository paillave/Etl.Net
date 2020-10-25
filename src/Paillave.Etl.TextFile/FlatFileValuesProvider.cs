using Paillave.Etl.Core;
using System;
using System.IO;
using System.Text;
using Paillave.Etl.TextFile.Core;
using System.Threading;
using Paillave.Etl.ValuesProviders;

namespace Paillave.Etl.TextFile
{
    public class FlatFileValuesProviderArgs<TParsed, TOut>
    {
        public FlatFileDefinition<TParsed> Mapping { get; set; }
        public Func<IFileValue, TParsed, TOut> ResultSelector { get; set; }
        public Encoding Encoding { get; set; } = null;
    }
    public class FlatFileValuesProvider<TParsed, TOut> : ValuesProviderBase<IFileValue, TOut>
    {
        private readonly FlatFileValuesProviderArgs<TParsed, TOut> _args;
        public FlatFileValuesProvider(FlatFileValuesProviderArgs<TParsed, TOut> args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public override void PushValues(IFileValue input, Action<TOut> push, CancellationToken cancellationToken, IDependencyResolver resolver)
        {
            var stream = input.GetContent();
            string sourceName = input.Name;
            var sr = _args.Encoding == null ? new StreamReader(stream, true) : new StreamReader(stream, _args.Encoding);
            using (sr)
            {
                if (_args.Mapping.HasColumnHeader)
                {
                    int index = 0;
                    LineSerializer<TParsed> lineSerializer = null;
                    while (!sr.EndOfStream)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
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
                                parsed = lineSerializer.Deserialize(line, sourceName, index);
                            }
                            catch (Exception ex)
                            {
                                throw new FlatFileLineDeserializeException(sourceName, index, ex);
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
                        if (cancellationToken.IsCancellationRequested) break;
                        string line = sr.ReadLine();

                        if (index >= _args.Mapping.FirstLinesToIgnore && !string.IsNullOrWhiteSpace(line))
                        {
                            TParsed parsed;
                            try
                            {
                                parsed = lineSerializer.Deserialize(line, sourceName, index);
                            }
                            catch (Exception ex)
                            {
                                throw new FlatFileLineDeserializeException(sourceName, index, ex);
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
