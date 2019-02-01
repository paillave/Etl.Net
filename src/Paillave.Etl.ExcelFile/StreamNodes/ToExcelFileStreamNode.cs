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
    public class ToExcelFileArgs<TIn>
    {
        public IStream<TIn> MainStream { get; set; }
        public ExcelFileDefinition<TIn> Mapping { get; set; }
    }
    public class ToExcelFileStreamNode<TIn> : StreamNodeBase<Stream, IStream<Stream>, ToExcelFileArgs<TIn>>
    {
        public ToExcelFileStreamNode(string name, ToExcelFileArgs<TIn> args) : base(name, args)
        {
        }

        protected override IStream<Stream> CreateOutputStream(ToExcelFileArgs<TIn> args)
        {
            var obs = args.MainStream.Observable.ToList().Map(ProcessValueToOutput);
            return CreateUnsortedStream(obs);
        }

        protected Stream ProcessValueToOutput(IList<TIn> value)
        {
            MemoryStream stream = new MemoryStream();
            value.WriteExcelListInStream(Args.Mapping, stream);
            return stream;
        }
    }
}
