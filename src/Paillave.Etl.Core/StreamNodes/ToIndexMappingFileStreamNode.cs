using Paillave.Etl.Helpers;
using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.Streams;
using System;
using System.Collections.Generic;
using System.Text;
using SystemIO = System.IO;

namespace Paillave.Etl.StreamNodes
{
    public class ToIndexMappingFileArgs<TIn> : ToResourceStreamArgsBase<SystemIO.StreamWriter> where TIn : new()
    {
        public ColumnIndexFlatFileDescriptor<TIn> Mapping { get; set; }
    }

    public class ToIndexMappingFileStreamNode<TIn> : ToResourceStreamNodeBase<TIn, SystemIO.StreamWriter, ToIndexMappingFileArgs<TIn>> where TIn : new()
    {
        private Func<TIn, IList<string>> _serialize;
        public ToIndexMappingFileStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, ToIndexMappingFileArgs<TIn> arguments) : base(input, name, parentNodeNamePath, arguments)
        {
            _serialize = this.Arguments.Mapping.ColumnIndexMappingConfiguration.LineSerializer();
        }

        protected override void ProcessValueToOutput(SystemIO.StreamWriter outputResource, TIn value)
        {
            outputResource.WriteLine(this.Arguments.Mapping.LineJoiner(_serialize(value)));
        }
    }
    public static partial class StreamEx
    {
        public static IStream<TIn> ToTextFile<TIn>(this IStream<TIn> stream, string name, IStream<SystemIO.StreamWriter> resourceStream, ColumnIndexFlatFileDescriptor<TIn> mapping) where TIn : new()
        {
            return new ToIndexMappingFileStreamNode<TIn>(stream, name, null, new ToIndexMappingFileArgs<TIn>
            {
                Mapping = mapping,
                ResourceStream = resourceStream
            }).Output;
        }
    }
}
