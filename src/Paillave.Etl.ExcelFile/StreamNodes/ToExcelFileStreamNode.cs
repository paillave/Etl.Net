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

namespace Paillave.Etl.ExcelFile.StreamNodes
{
    public class ToExcelFileArgs<TIn, TStream>
        //where TIn : new()
        where TStream : IStream<TIn>
    {
        public TStream MainStream { get; set; }
        public IStream<Stream> TargetStream { get; set; }
        //public ExcelFileDefinition<TIn> Mapping { get; set; }
    }
    public class ToExcelFileStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToExcelFileArgs<TIn, TStream>>
        //where TIn : new()
        where TStream : IStream<TIn>
    {
        public override bool IsAwaitable => true;

        public ToExcelFileStreamNode(string name, ToExcelFileArgs<TIn, TStream> args) : base(name, args)
        {
        }

        protected override TStream CreateOutputStream(ToExcelFileArgs<TIn, TStream> args)
        {
            var firstStreamWriter = args.TargetStream.Observable.First().DelayTillEndOfStream();
            var obs = args.MainStream.Observable.ToList()
                .CombineWithLatest(firstStreamWriter, (i, r) => { ProcessValueToOutput(r, i); return i; }, true)
                //.CombineWithLatest(firstStreamWriter, (i, r) => { ProcessValueToOutput(r, args.Mapping, i); return i; }, true)
                .FlatMap(i => PushObservable.FromEnumerable(i));
            return CreateMatchingStream(obs, args.MainStream);
        }
        //protected void ProcessValueToOutput(SystemIO.StreamWriter streamWriter, ExcelFileDefinition<TIn> mapping, IList<TIn> value)
        protected void ProcessValueToOutput(Stream streamWriter, IList<TIn> value)
        {
            var pck = new ExcelPackage();
            var wsList = pck.Workbook.Worksheets.Add("Sheet1");
            var r1 = wsList.Cells["A1"].LoadFromCollection(value, true, TableStyles.Medium11);
            r1.AutoFitColumns();
            BinaryWriter bw = new BinaryWriter(streamWriter);
            bw.Write(pck.GetAsByteArray());
        }
    }
}
