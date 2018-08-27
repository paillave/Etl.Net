using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Helpers;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Text;
using SystemIO = System.IO;

namespace Paillave.Etl.StreamNodes
{
    public class ToNameMappingFileArgs<TIn, TStream>
        where TIn : new()
        where TStream : IStream<TIn>
    {
        public TStream MainStream { get; set; }
        public IStream<SystemIO.StreamWriter> TargetStream { get; set; }
        public ColumnNameFlatFileDescriptor<TIn> Mapping { get; set; }
    }
    public class ToNameMappingFileStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToNameMappingFileArgs<TIn, TStream>>
        where TIn : new()
        where TStream : IStream<TIn>
    {
        private Func<TIn, IList<string>> _serialize;

        public ToNameMappingFileStreamNode(string name, ToNameMappingFileArgs<TIn, TStream> args) : base(name, args)
        {
            _serialize = args.Mapping.ColumnNameMappingConfiguration.LineSerializer();
        }

        protected override TStream CreateOutputStream(ToNameMappingFileArgs<TIn, TStream> args)
        {
            var firstStreamWriter = args.TargetStream.Observable.First().Do(i => PreProcess(i, args.Mapping)).DelayTillEndOfStream();
            var obs = args.MainStream.Observable
                .CombineWithLatest(firstStreamWriter, (i, r) => { ProcessValueToOutput(r, args.Mapping, i); return i; }, true);
            return CreateMatchingStream(obs, args.MainStream);
        }
        private void PreProcess(SystemIO.StreamWriter streamWriter, ColumnNameFlatFileDescriptor<TIn> mapping)
        {
            streamWriter.WriteLine(mapping.LineJoiner(mapping.ColumnNameMappingConfiguration.GetHeaders()));
        }
        protected void ProcessValueToOutput(SystemIO.StreamWriter streamWriter, ColumnNameFlatFileDescriptor<TIn> mapping, TIn value)
        {
            streamWriter.WriteLine(mapping.LineJoiner(_serialize(value)));
        }
    }
}
