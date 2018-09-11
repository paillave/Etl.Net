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
    public class ToExcelFileArgs<TIn, TStream>
        where TStream : IStream<TIn>
    {
        public TStream MainStream { get; set; }
        public IStream<Stream> TargetStream { get; set; }
        public ExcelFileDefinition<TIn> Mapping { get; set; }
    }
    public class ToExcelFileStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToExcelFileArgs<TIn, TStream>>
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
            var excelColumns = base.Args.Mapping.GetExcelReader().PropertySetters.OrderBy(i => i.Key).Select(i => i.Value).ToArray();
            var wsList = pck.Workbook.Worksheets.Add("Sheet1");
            var r1 = wsList.Cells["A1"].LoadFromCollection(value, true, TableStyles.Medium11, BindingFlags.Instance | BindingFlags.Public, excelColumns.Select(i => i.PropertyInfo).ToArray());

            var table = wsList.Tables.GetFromRange(r1);

            foreach (var item in excelColumns.Select((i, idx) => new { ColumnIndex = idx, Label = i.ColumnName ?? i.PropertyInfo.Name }))
                table.Columns[item.ColumnIndex].Name = item.Label;

            r1.AutoFitColumns();
            BinaryWriter bw = new BinaryWriter(streamWriter);
            bw.Write(pck.GetAsByteArray());
        }
    }
}
