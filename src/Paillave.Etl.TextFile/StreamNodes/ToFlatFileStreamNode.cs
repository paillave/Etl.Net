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
    public class ToFlatFileArgs<TIn>
    {
        public IStream<TIn> MainStream { get; set; }
        public FlatFileDefinition<TIn> Mapping { get; set; }
    }
    public class ToFlatFileStreamNode<TIn> : StreamNodeBase<Stream, IStream<Stream>, ToFlatFileArgs<TIn>>
    {
        private readonly LineSerializer<TIn> _serialize;
        private StreamWriter _streamWriter;
        private Stream _stream;

        public ToFlatFileStreamNode(string name, ToFlatFileArgs<TIn> args) : base(name, args)
        {
            _stream = new MemoryStream();
            _streamWriter = new StreamWriter(_stream, Encoding.Default, 1024, true);
            this.ExecutionContext.AddDisposable(_streamWriter);
            _streamWriter.WriteLine(args.Mapping.GenerateDefaultHeaderLine());
            _serialize = args.Mapping.GetSerializer();
        }

        protected override IStream<Stream> CreateOutputStream(ToFlatFileArgs<TIn> args)
        {
            var obs = args.MainStream.Observable.Do(ProcessValueToOutput).Last().Map(i =>
            {
                // _streamWriter.Dispose();
                return _stream;
            });
            return CreateUnsortedStream(obs);
        }
        private void ProcessValueToOutput(TIn value)
        {
            _streamWriter.WriteLine(_serialize.Serialize(value));
        }
    }
}
