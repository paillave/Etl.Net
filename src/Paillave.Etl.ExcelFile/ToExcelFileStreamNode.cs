using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.ExcelFile.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Paillave.Etl.ExcelFile
{
    public class ToExcelFileArgs<TIn>
    {
        public IStream<TIn> MainStream { get; set; }
        public ExcelFileDefinition<TIn> Mapping { get; set; }
        public string FileName { get; set; }
    }
    public class ToExcelFileStreamNode<TIn>(string name, ToExcelFileArgs<TIn> args) : StreamNodeBase<IFileValue, IStream<IFileValue>, ToExcelFileArgs<TIn>>(name, args)
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;
        protected override IStream<IFileValue> CreateOutputStream(ToExcelFileArgs<TIn> args)
        {
            var obs = args.MainStream.Observable.ToList().Map(ProcessValueToOutput);
            return CreateUnsortedStream(obs);
        }

        protected IFileValue ProcessValueToOutput(IList<TIn> value)
        {
            MemoryStream stream = new MemoryStream();
            var excelReader = Args.Mapping.GetExcelReader();
            value.WriteExcelListInStream(excelReader, stream);
            return FileValue.Create(stream, this.Args.FileName, new ExcelFileValueMetadata
            {
                Map = excelReader.GetTextMapping()
            });
        }
    }
    public class ExcelFileValueMetadata : FileValueMetadataBase
    {
        public Dictionary<string, string> Map { get; set; }
    }
    public class ToExcelFileArgs<TIn, TStream>
        where TStream : IStream<TIn>
    {
        public TStream MainStream { get; set; }
        public ExcelFileDefinition<TIn> Mapping { get; set; }
        public ISingleStream<Stream> TargetStream { get; set; }
    }
    public class ToExcelFileStreamNode<TIn, TStream>(string name, ToExcelFileArgs<TIn, TStream> args) : StreamNodeBase<TIn, TStream, ToExcelFileArgs<TIn, TStream>>(name, args)
        where TStream : IStream<TIn>
    {
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Average;

        protected override TStream CreateOutputStream(ToExcelFileArgs<TIn, TStream> args)
        {
            var firstStreamWriter = args.TargetStream.Observable.First().DelayTillEndOfStream();
            var obs = args.MainStream.Observable.ToList()
                .CombineWithLatest(firstStreamWriter, (i, r) => { ProcessValueToOutput(r, i); return i; }, true)
                .FlatMap((i, ct) => PushObservable.FromEnumerable(i, ct));
            return CreateMatchingStream(obs, args.MainStream);
        }
        protected void ProcessValueToOutput(Stream streamWriter, IList<TIn> value) => value.WriteExcelListInStream(Args.Mapping, streamWriter);
    }
}
