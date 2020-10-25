using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.ValuesProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Paillave.Etl.FileSystem
{
    public class WriteToFileArgs<TParams>
    {
        public IStream<IFileValue> Stream { get; set; }
        public ISingleStream<TParams> ParamStream { get; set; }
        public Func<TParams, string> GetOutputFilePath { get; set; }
    }
    /// <summary>
    /// Write a data Stream into a file
    /// </summary>
    /// <typeparam name="TParams"></typeparam>
    public class WriteToFileStreamNode<TParams> : StreamNodeBase<IFileValue, IStream<IFileValue>, WriteToFileArgs<TParams>>
    {
        public WriteToFileStreamNode(string name, WriteToFileArgs<TParams> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => throw new NotImplementedException();

        public override ProcessImpact MemoryFootPrint => throw new NotImplementedException();

        protected override IStream<IFileValue> CreateOutputStream(WriteToFileArgs<TParams> args)
        {
            var outputFilePath = args.ParamStream.Observable.Map(args.GetOutputFilePath);
            var outputObservable = args.Stream.Observable.CombineWithLatest(outputFilePath, (fileValue, r) =>
            {
                var l = fileValue.GetContent();
                l.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.OpenWrite(r))
                    l.CopyTo(fileStream);
                return fileValue;
            }, true);
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
