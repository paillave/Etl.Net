using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Helpers;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.Text;
using SystemIO = System.IO;

namespace Paillave.Etl.StreamNodes
{
    public class ToIndexMappingFileArgs<TIn, TStream>
        where TIn : new()
        where TStream : IStream<TIn>
    {
        public TStream MainStream { get; set; }
        public IStream<SystemIO.StreamWriter> TargetStream { get; set; }
        public ColumnIndexFlatFileDescriptor<TIn> Mapping { get; set; }
    }
    public class ToIndexMappingFileStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToIndexMappingFileArgs<TIn, TStream>>
        where TIn : new()
        where TStream : IStream<TIn>
    {
        private readonly Func<TIn, IList<string>> _serialize;
        public override bool IsAwaitable => true;

        public ToIndexMappingFileStreamNode(string name, ToIndexMappingFileArgs<TIn, TStream> args) : base(name, args)
        {
            _serialize = args.Mapping.ColumnIndexMappingConfiguration.LineSerializer();
        }

        protected override TStream CreateOutputStream(ToIndexMappingFileArgs<TIn, TStream> args)
        {
            return CreateMatchingStream(args.MainStream.Observable
                .CombineWithLatest(args.TargetStream.Observable.First(), (i, r) => { ProcessValueToOutput(r, args.Mapping, i); return i; }, true), args.MainStream);
        }
        protected void ProcessValueToOutput(SystemIO.StreamWriter streamWriter, ColumnIndexFlatFileDescriptor<TIn> mapping, TIn value)
        {
            streamWriter.WriteLine(mapping.LineJoiner(_serialize(value)));
        }
    }
}
