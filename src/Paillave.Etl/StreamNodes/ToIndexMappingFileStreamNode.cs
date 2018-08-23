using Paillave.Etl.Helpers;
using Paillave.Etl.Core.StreamNodesOld;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using SystemIO = System.IO;

namespace Paillave.Etl.StreamNodes
{
    public class ToIndexMappingFileArgs<TIn> : ToStreamFromOneContextValueArgsBase<SystemIO.StreamWriter> where TIn : new()
    {
        public ToIndexMappingFileArgs(IStream<SystemIO.StreamWriter> contextStream, ColumnIndexFlatFileDescriptor<TIn> mapping) : base(contextStream)
        {
            this.Mapping = mapping;
        }

        public ColumnIndexFlatFileDescriptor<TIn> Mapping { get; }
    }

    public class ToIndexMappingFileStreamNode<TIn> : ToStreamFromOneResourceContextValueNodeBase<TIn, SystemIO.StreamWriter, SystemIO.StreamWriter, ToIndexMappingFileArgs<TIn>> where TIn : new()
    {
        private Func<TIn, IList<string>> _serialize;
        public ToIndexMappingFileStreamNode(IStream<TIn> input, string name, ToIndexMappingFileArgs<TIn> arguments) : base(input, name, arguments)
        {
            _serialize = this.Arguments.Mapping.ColumnIndexMappingConfiguration.LineSerializer();
        }

        protected override void ProcessValueToOutput(SystemIO.StreamWriter outputResource, TIn value)
        {
            outputResource.WriteLine(this.Arguments.Mapping.LineJoiner(_serialize(value)));
        }
    }
}
