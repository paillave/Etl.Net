using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using Paillave.Etl.Reactive.Operators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Paillave.Etl.StreamNodes
{
    public class ToLocalFileArgs<TParams>
    {
        public IStream<Stream> Stream { get; set; }
        public IStream<TParams> ParamStream { get; set; }
        public Func<TParams, string> GetOutputFilePath { get; set; }
    }
    public class ToLocalFileStreamNode<TParams> : StreamNodeBase<Stream, IStream<Stream>, ToLocalFileArgs<TParams>>
    {
        public override bool IsAwaitable => true;
        public ToLocalFileStreamNode(string name, ToLocalFileArgs<TParams> args) : base(name, args)
        {
        }
        protected override IStream<Stream> CreateOutputStream(ToLocalFileArgs<TParams> args)
        {
            var outputFilePath = args.ParamStream.Observable.Map(args.GetOutputFilePath);
            var outputObservable = args.Stream.Observable.CombineWithLatest(outputFilePath, (l, r) =>
            {
                l.Seek(0, SeekOrigin.Begin);
                using (var fileStream = File.OpenWrite(r))
                    l.CopyTo(fileStream);
                return l;
            }, true);
            return base.CreateUnsortedStream(outputObservable);
        }
    }
}
