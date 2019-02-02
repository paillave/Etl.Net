using Paillave.Etl.Core.Streams;
using Paillave.Etl.StreamNodes;
using Paillave.Etl.ValuesProviders;
using System;
using System.IO;
using System.Linq;
using SystemIO = System.IO;

namespace Paillave.Etl.Extensions
{
    public static class ThroughLocalFileEx
    {
        public static IStream<Stream> ThroughLocalFile(this IStream<Stream> stream, string name, IStream<string> outputFilePathStream)
        {
            return new ThroughLocalFileStreamNode<string>(name, new ThroughLocalFileArgs<string>
            {
                GetOutputFilePath = i => i,
                ParamStream = outputFilePathStream,
                Stream = stream
            }).Output;
        }
        public static IStream<Stream> ThroughLocalFile<TParam>(this IStream<Stream> stream, string name, IStream<TParam> outputFilePathStream, Func<TParam, string> getOutputFilePath)
        {
            return new ThroughLocalFileStreamNode<TParam>(name, new ThroughLocalFileArgs<TParam>
            {
                GetOutputFilePath = getOutputFilePath,
                ParamStream = outputFilePathStream,
                Stream = stream
            }).Output;
        }
    }
}
