using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.ExcelFile.Core;
using System;
using System.Collections.Generic;
using System.Text;
using SystemIO = System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Paillave.Etl.ExcelFile.StreamNodes
{
    public class ThroughExcelFileArgs<TIn, TStream>
        where TStream : IStream<TIn>
    {
        public TStream MainStream { get; set; }
        public ExcelFileDefinition<TIn> Mapping { get; set; }
        public ISingleStream<Stream> TargetStream { get; set; }
    }
    public class ThroughExcelFileStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ThroughExcelFileArgs<TIn, TStream>>
        where TStream : IStream<TIn>
    {
        public ThroughExcelFileStreamNode(string name, ThroughExcelFileArgs<TIn, TStream> args) : base(name, args)
        {
        }

        protected override TStream CreateOutputStream(ThroughExcelFileArgs<TIn, TStream> args)
        {
            var firstStreamWriter = args.TargetStream.Observable.First().DelayTillEndOfStream();
            var obs = args.MainStream.Observable.ToList()
                .CombineWithLatest(firstStreamWriter, (i, r) => { ProcessValueToOutput(r, i); return i; }, true)
                .FlatMap(i => PushObservable.FromEnumerable(i));
            return CreateMatchingStream(obs, args.MainStream);
        }
        protected void ProcessValueToOutput(Stream streamWriter, IList<TIn> value)
        {
            value.WriteExcelListInStream(Args.Mapping, streamWriter);
        }
    }
}
