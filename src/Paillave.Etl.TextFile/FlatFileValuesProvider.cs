using Paillave.Etl.Core;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Paillave.Etl.TextFile;

public class FlatFileValuesProviderArgs<TParsed, TOut>
{
    public FlatFileDefinition<TParsed> Mapping { get; set; }
    public Func<IFileValue, TParsed, TOut> ResultSelector { get; set; }
    public Encoding Encoding { get; set; } = null;
    public bool UseStreamCopy { get; set; } = true;
}
public class FlatFileValuesProvider<TParsed, TOut>(FlatFileValuesProviderArgs<TParsed, TOut> args) : ValuesProviderBase<IFileValue, TOut>
{
    private readonly FlatFileValuesProviderArgs<TParsed, TOut> _args = args;

    public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
    public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
    public override void PushValues(IFileValue input, Action<TOut> push, CancellationToken cancellationToken, IExecutionContext context)
    {
        using var stream = input.Get(_args.UseStreamCopy);
        string sourceName = input.Name;
        var encoding = _args.Encoding ?? _args.Mapping.Encoding;
        using var sr = encoding == null ? new StreamReader(stream, true) : new StreamReader(stream, encoding);
        if (_args.Mapping.HasColumnHeader)
        {
            int index = 0;
            LineSerializer<TParsed> lineSerializer = null;
            while (!sr.EndOfStream)
            {
                if (cancellationToken.IsCancellationRequested) break;
                string line = sr.ReadLine();
                if (_args.Mapping.LinePreProcessor != null)
                {
                    line = _args.Mapping.LinePreProcessor(line);
                }
                if (index == _args.Mapping.FirstLinesToIgnore)
                {
                    lineSerializer = _args.Mapping.GetSerializer(line);
                }
                else if (index > _args.Mapping.FirstLinesToIgnore && !string.IsNullOrWhiteSpace(line))
                {
                    TParsed parsed;
                    try
                    {
                        parsed = lineSerializer.Deserialize(line, sourceName, index, _args.Mapping.ValuePreProcessor);
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
                        parsed = lineSerializer.Deserialize(line, sourceName, index, _args.Mapping.ValuePreProcessor);
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
