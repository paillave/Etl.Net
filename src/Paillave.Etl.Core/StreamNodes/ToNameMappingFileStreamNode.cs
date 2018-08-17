using Paillave.Etl.Helpers;
using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using SystemIO = System.IO;

namespace Paillave.Etl.StreamNodes
{
    public class ToNameMappingFileArgs<TIn> : ToResourceStreamArgsBase<SystemIO.StreamWriter> where TIn : new()
    {
        public ColumnNameFlatFileDescriptor<TIn> Mapping { get; set; }
    }

    public class ToNameMappingFileStreamNode<TIn> : ToResourceStreamNodeBase<TIn, SystemIO.StreamWriter, ToNameMappingFileArgs<TIn>> where TIn : new()
    {
        private Func<TIn, IList<string>> _serialize;
        public ToNameMappingFileStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, ToNameMappingFileArgs<TIn> arguments) : base(input, name, parentNodeNamePath, arguments)
        {
            _serialize = this.Arguments.Mapping.ColumnNameMappingConfiguration.LineSerializer();
        }

        protected override void ProcessValueToOutput(SystemIO.StreamWriter outputResource, TIn value)
        {
            outputResource.WriteLine(this.Arguments.Mapping.LineJoiner(_serialize(value)));
        }
    }
}
