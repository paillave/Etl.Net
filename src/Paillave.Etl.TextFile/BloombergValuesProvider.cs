using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Paillave.Etl.TextFile
{
    public class BloombergResult<TParsed>
    {
        public string Ticker { get; set; }
        public TParsed Values { get; set; }
        public Dictionary<string, DateTime> ExtraDates { get; set; }
    }
    public class BloombergValuesProviderArgs<TParsed>
    {
        public FlatFileDefinition<TParsed> Mapping { get; set; }
        public Encoding Encoding { get; set; } = null;
    }
    public static class BloombergValuesProvider
    {
        public static BloombergValuesProvider<TParsed> Create<TParsed>(FlatFileDefinition<TParsed> mapping)
        {
            return new BloombergValuesProvider<TParsed>(new BloombergValuesProviderArgs<TParsed>
            {
                Mapping = mapping,
            });
        }
    }
    public class BloombergValuesProvider<TParsed> : ValuesProviderBase<IFileValue, BloombergResult<TParsed>>
    {
        private readonly BloombergValuesProviderArgs<TParsed> _args;
        public BloombergValuesProvider(BloombergValuesProviderArgs<TParsed> args) => _args = args;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        private enum FileReadState
        {
            None = 1,
            Fields = 2,
            Data = 3
        }
        public override void PushValues(IFileValue input, Action<BloombergResult<TParsed>> push, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            var stream = input.GetContent();
            string sourceName = input.Name;
            var encoding = _args.Encoding ?? _args.Mapping.Encoding;
            var sr = encoding == null ? new StreamReader(stream, true) : new StreamReader(stream, encoding);
            using (sr)
            {
                var extraColumns = new Dictionary<string, DateTime>();
                var extraColumnIndex = 0;
                int index = 0;
                var columnCollection = new List<string>();
                LineSerializer<TParsed> lineSerializer = null;
                var currentState = FileReadState.None;
                var regexColmunWithExtra = new Regex("(?<name>[^:]+):(?<date>\\d+):P", RegexOptions.Compiled & RegexOptions.IgnoreCase);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    switch (currentState)
                    {
                        case FileReadState.None:
                            if (line == "START-OF-FIELDS")
                            {
                                currentState = FileReadState.Fields;
                            }
                            else if (line == "START-OF-DATA")
                            {
                                currentState = FileReadState.Data;
                                lineSerializer = _args.Mapping.IsColumnSeparated('|').GetSerializer(columnCollection);
                            }
                            break;
                        case FileReadState.Fields:
                            if (line == "END-OF-FIELDS")
                            {
                                currentState = FileReadState.None;
                            }
                            else
                            {
                                var extraColumnMatch = regexColmunWithExtra.Match(line);
                                if (extraColumnMatch.Success)
                                {
                                    line = $"{extraColumnMatch.Groups["name"].Value}:{++extraColumnIndex}";
                                    extraColumns[$"{extraColumnMatch.Groups["name"].Value}:{extraColumnIndex}"] = DateTime.ParseExact(extraColumnMatch.Groups["date"].Value, "yyyyMMdd", CultureInfo.InvariantCulture);
                                }
                                columnCollection.Add(line);
                            }
                            break;
                        case FileReadState.Data:
                            if (line == "END-OF-DATA")
                            {
                                currentState = FileReadState.None;
                            }
                            else
                            {
                                line = line.Replace("N.A.", "");
                                TParsed parsed;
                                string ticker = null;
                                try
                                {
                                    var splitted = lineSerializer.Splitter.Split(line);
                                    ticker = splitted[0];
                                    line = string.Join("|", splitted.Skip(3));
                                    parsed = lineSerializer.Deserialize(line, sourceName, index);
                                }
                                catch (Exception ex)
                                {
                                    throw new FlatFileLineDeserializeException(sourceName, index, ex);
                                }
                                push(new BloombergResult<TParsed>
                                {
                                    ExtraDates = extraColumns,
                                    Ticker = ticker,
                                    Values = parsed,
                                });
                                index++;
                            }
                            break;
                    }
                }
            }
        }
    }
}
