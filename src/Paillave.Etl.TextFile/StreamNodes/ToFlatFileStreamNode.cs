using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.TextFile.Core;
using System;
using System.Collections.Generic;
using System.Text;
using SystemIO = System.IO;

namespace Paillave.Etl.TextFile.StreamNodes
{
    public class ToFlatFileArgs<TIn, TStream>
        where TIn : new()
        where TStream : IStream<TIn>
    {
        public TStream MainStream { get; set; }
        public IStream<SystemIO.StreamWriter> TargetStream { get; set; }
        public FileDefinition<TIn> Mapping { get; set; }
    }
    public class ToFlatFileStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToFlatFileArgs<TIn, TStream>>
        where TIn : new()
        where TStream : IStream<TIn>
    {
        private readonly LineSerializer<TIn> _serialize;
        public override bool IsAwaitable => true;

        public ToFlatFileStreamNode(string name, ToFlatFileArgs<TIn, TStream> args) : base(name, args)
        {
            _serialize = args.Mapping.GetSerializer();
        }

        protected override TStream CreateOutputStream(ToFlatFileArgs<TIn, TStream> args)
        {
            var firstStreamWriter = args.TargetStream.Observable.First().Do(i => PreProcess(i, args.Mapping)).DelayTillEndOfStream();
            var obs = args.MainStream.Observable
                .CombineWithLatest(firstStreamWriter, (i, r) => { ProcessValueToOutput(r, args.Mapping, i); return i; }, true);
            return CreateMatchingStream(obs, args.MainStream);
        }
        private void PreProcess(SystemIO.StreamWriter streamWriter, FileDefinition<TIn> mapping)
        {
            streamWriter.WriteLine(mapping.GenerateDefaultHeaderLine());
        }
        protected void ProcessValueToOutput(SystemIO.StreamWriter streamWriter, FileDefinition<TIn> mapping, TIn value)
        {
            streamWriter.WriteLine(_serialize.Serialize(value));
        }
    }
}
