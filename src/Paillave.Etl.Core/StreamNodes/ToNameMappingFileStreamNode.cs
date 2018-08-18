using Paillave.Etl.Helpers;
using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using SystemIO = System.IO;
using Paillave.RxPush.Core;

namespace Paillave.Etl.StreamNodes
{
    public class ToNameMappingFileArgs<TIn> : ToResourceStreamArgsBase<SystemIO.StreamWriter> where TIn : new()
    {
        public ColumnNameFlatFileDescriptor<TIn> Mapping { get; set; }
    }

    public class ToNameMappingFileStreamNode<TIn> : ToResourceStreamNodeBase<TIn, SystemIO.StreamWriter, ToNameMappingFileArgs<TIn>> where TIn : new()
    {
        private Func<TIn, IList<string>> _serialize;
        public ToNameMappingFileStreamNode(IStream<TIn> input, string name, ToNameMappingFileArgs<TIn> arguments) : base(input, name, arguments)
        {
            _serialize = this.Arguments.Mapping.ColumnNameMappingConfiguration.LineSerializer();
        }

        protected override void PreProcess(SystemIO.StreamWriter outputResource)
        {
            outputResource.WriteLine(this.Arguments.Mapping.LineJoiner(this.Arguments.Mapping.ColumnNameMappingConfiguration.GetHeaders()));
        }

        protected override void ProcessValueToOutput(SystemIO.StreamWriter outputResource, TIn value)
        {
            outputResource.WriteLine(this.Arguments.Mapping.LineJoiner(_serialize(value)));
        }
    }
}
