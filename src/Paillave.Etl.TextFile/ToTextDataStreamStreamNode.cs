using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SystemIO = System.IO;

namespace Paillave.Etl.TextFile
{
    public class ToTextDataStreamArgs<TIn, TRow>
    {
        public IStream<TIn> MainStream { get; set; }
        public Func<TIn, TRow> GetRow { get; set; }
        public FlatFileDefinition<TRow> Mapping { get; set; }
        public string FileName { get; set; }
        public Encoding Encoding { get; set; }
        public Dictionary<string, List<Destination>> Destinations { get; set; }
        public object Metadata { get; set; }
    }
    /// <summary>
    /// Writes what goes in the stream into a structured flat file
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    public class ToFileValueStreamNode<TIn, TRow> : StreamNodeBase<IFileValue, ISingleStream<IFileValue>, ToTextDataStreamArgs<TIn, TRow>>
    {
        private readonly LineSerializer<TRow> _serialize;
        private FileValueWriter<FlatFileValueMetadata> _streamWriter;
        public ToFileValueStreamNode(string name, ToTextDataStreamArgs<TIn, TRow> args) : base(name, args)
        {
            _serialize = args.Mapping.GetSerializer();
            _streamWriter = FileValueWriter.Create(new FlatFileValueMetadata
            {
                Map = _serialize.GetTextMapping(),
                ExtraMetadata = args.Metadata,
                Destinations = args.Destinations,
            }, args.FileName, args.Encoding ?? Encoding.Default, 1024);
            _streamWriter.WriteLine(args.Mapping.GenerateDefaultHeaderLine());
        }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override ISingleStream<IFileValue> CreateOutputStream(ToTextDataStreamArgs<TIn, TRow> args)
        {
            var obs = args.MainStream.Observable.Do(ProcessValueToOutput).Completed().Map(i => _streamWriter);
            return CreateSingleStream(obs);
        }
        private void ProcessValueToOutput(TIn value) => _streamWriter.WriteLine(_serialize.Serialize(Args.GetRow(value)));
    }
    public class FlatFileValueMetadata : FileValueMetadataBase, IFileValueWithDestinationMetadata
    {
        public Dictionary<string, string> Map { get; set; }
        public Dictionary<string, List<Destination>> Destinations { get; set; }
        public object ExtraMetadata { get; set; }
    }
    public class ToTextDataStreamFileArgs<TIn, TStream>
        where TStream : IStream<TIn>
    {
        public TStream MainStream { get; set; }
        public ISingleStream<SystemIO.Stream> TargetDataStream { get; set; }
        public FlatFileDefinition<TIn> Mapping { get; set; }
    }
    /// <summary>
    /// Write a structured flat file in a data Stream
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TStream"></typeparam>
    public class ToTextDataStreamStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToTextDataStreamFileArgs<TIn, TStream>>
        where TStream : IStream<TIn>
    {
        private readonly LineSerializer<TIn> _serialize;
        private StreamWriter _streamWriter;
        public override ProcessImpact PerformanceImpact => ProcessImpact.Average;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public ToTextDataStreamStreamNode(string name, ToTextDataStreamFileArgs<TIn, TStream> args) : base(name, args)
        {
            _serialize = args.Mapping.GetSerializer();
        }
        protected override TStream CreateOutputStream(ToTextDataStreamFileArgs<TIn, TStream> args)
        {
            var firstStreamWriter = args.TargetDataStream.Observable.First().Do(i => PreProcess(i, args.Mapping)).DelayTillEndOfStream();
            var obs = args.MainStream.Observable
                .CombineWithLatest(firstStreamWriter, (i, r) => { ProcessValueToOutput(r, args.Mapping, i); return i; }, true);
            return CreateMatchingStream(obs, args.MainStream);
        }
        private void PreProcess(SystemIO.Stream stream, FlatFileDefinition<TIn> mapping)
        {
            _streamWriter = new StreamWriter(stream, Encoding.Default, 1024, true);
            this.ExecutionContext.AddDisposable(_streamWriter);
            _streamWriter.WriteLine(mapping.GenerateDefaultHeaderLine());
        }
        protected void ProcessValueToOutput(SystemIO.Stream stream, FlatFileDefinition<TIn> mapping, TIn value)
        {
            _streamWriter.WriteLine(_serialize.Serialize(value));
        }

        protected override void PostProcess()
        {
            // if (_streamWriter != null)
            //     _streamWriter.Dispose();
        }
    }
}
