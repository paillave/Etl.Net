using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.IO;

namespace Paillave.Etl.FileSystem
{
    [Obsolete("KISS & YAGNI")]
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
    [Obsolete("KISS & YAGNI")]
    public class WriteToFileStreamNode<TParams> : StreamNodeBase<IFileValue, IStream<IFileValue>, WriteToFileArgs<TParams>>
    {
        public WriteToFileStreamNode(string name, WriteToFileArgs<TParams> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;
        protected override IStream<IFileValue> CreateOutputStream(WriteToFileArgs<TParams> args)
        {
            var outputFilePath = args.ParamStream.Observable.Map(args.GetOutputFilePath);
            var outputObservable = args.Stream.Observable.CombineWithLatest(outputFilePath, (fileValue, r) =>
            {
                var l = fileValue.GetContent();
                l.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.Open(r, FileMode.Create))
                    l.CopyTo(fileStream);
                return fileValue;
            }, true);
            return base.CreateUnsortedStream(outputObservable);
        }
    }
    public class WriteToFileArgs
    {
        public IStream<IFileValue> Stream { get; set; }
        public Func<IFileValue, string> GetOutputFilePath { get; set; }
    }
    /// <summary>
    /// Write a data Stream into a file
    /// </summary>
    /// <typeparam name="TParams"></typeparam>
    public class WriteToFileStreamNode : StreamNodeBase<IFileValue, IStream<IFileValue>, WriteToFileArgs>
    {
        public WriteToFileStreamNode(string name, WriteToFileArgs args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Heavy;
        protected override IStream<IFileValue> CreateOutputStream(WriteToFileArgs args)
        {
            var outputObservable = args.Stream.Observable.Do<IFileValue>(fileValue =>
            {
                var l = fileValue.GetContent();
                l.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.Open(args.GetOutputFilePath(fileValue), FileMode.Create))
                    l.CopyTo(fileStream);
            });
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
