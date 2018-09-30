using Paillave.Etl.Core.Streams;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.ValuesProviders;
using System;
using System.IO;
using System.Linq;
using SystemIO = System.IO;

namespace Paillave.Etl
{
    public static class ToLocalFileEx
    {
        public static IStream<Stream> ToLocalFile(this IStream<Stream> stream, string name, IStream<string> outputFilePathStream)
        {
            return new ToLocalFileStreamNode<string>(name, new ToLocalFileArgs<string>
            {
                GetOutputFilePath = i => i,
                ParamStream = outputFilePathStream,
                Stream = stream
            }).Output;
        }
        public static IStream<Stream> ToLocalFile<TParam>(this IStream<Stream> stream, string name, IStream<TParam> outputFilePathStream, Func<TParam, string> getOutputFilePath)
        {
            return new ToLocalFileStreamNode<TParam>(name, new ToLocalFileArgs<TParam>
            {
                GetOutputFilePath = getOutputFilePath,
                ParamStream = outputFilePathStream,
                Stream = stream
            }).Output;
        }
    }
}
