using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.TextFile.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SystemIO = System.IO;

namespace Paillave.Etl.TextFile.StreamNodes
{
    public class ThroughFlatFileArgs<TIn, TStream>
        where TStream : IStream<TIn>
    {
        public TStream MainStream { get; set; }
        public ISingleStream<SystemIO.Stream> TargetStream { get; set; }
        public FlatFileDefinition<TIn> Mapping { get; set; }
    }
    public class ThroughFlatFileStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ThroughFlatFileArgs<TIn, TStream>>
        where TStream : IStream<TIn>
    {
        private readonly LineSerializer<TIn> _serialize;
        private StreamWriter _streamWriter;

        public ThroughFlatFileStreamNode(string name, ThroughFlatFileArgs<TIn, TStream> args) : base(name, args)
        {
            _serialize = args.Mapping.GetSerializer();
        }

        protected override TStream CreateOutputStream(ThroughFlatFileArgs<TIn, TStream> args)
        {
            var firstStreamWriter = args.TargetStream.Observable.First().Do(i => PreProcess(i, args.Mapping)).DelayTillEndOfStream();
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
