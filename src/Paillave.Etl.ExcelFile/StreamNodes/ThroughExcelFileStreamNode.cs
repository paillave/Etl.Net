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
        public IStream<Stream> TargetStream { get; set; }
        public ExcelFileDefinition<TIn> Mapping { get; set; }
    }
    public class ThroughExcelFileStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ThroughExcelFileArgs<TIn, TStream>>
        where TStream : IStream<TIn>
    {
        public override bool IsAwaitable => true;

        public ThroughExcelFileStreamNode(string name, ThroughExcelFileArgs<TIn, TStream> args) : base(name, args)
        {
        }

        protected override TStream CreateOutputStream(ThroughExcelFileArgs<TIn, TStream> args)
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
            var style = pck.Workbook.Styles.CreateNamedStyle("Date");
            style.Style.Numberformat.Format = "dd/MM/yyyy";
            var wsList = pck.Workbook.Worksheets.Add("Sheet1");
            if (base.Args.Mapping != null)
            {
                var excelColumns = base.Args.Mapping.GetExcelReader().PropertySetters.OrderBy(i => i.Key).Select(i => i.Value).ToArray();
                var r1 = wsList.Cells["A1"].LoadFromCollection(value, true, TableStyles.Medium11, BindingFlags.Instance | BindingFlags.Public, excelColumns.Select(i => i.PropertyInfo).ToArray());
                var table = wsList.Tables.GetFromRange(r1);
                foreach (var item in excelColumns.Select((i, idx) => new { ColumnIndex = idx, Label = i.ColumnName ?? i.PropertyInfo.Name, Type = i.PropertyInfo.PropertyType }))
                {
                    table.Columns[item.ColumnIndex].Name = item.Label;
                    if (item.Type == typeof(DateTime))
                        table.Columns[item.ColumnIndex].DataCellStyleName = "Date";
                }
                r1.AutoFitColumns();
            }
            else
            {
                var properties = typeof(TIn).GetProperties();
                var r1 = wsList.Cells["A1"].LoadFromCollection(value, true, TableStyles.Medium11, BindingFlags.Instance | BindingFlags.Public, properties);
                var table = wsList.Tables.GetFromRange(r1);
                foreach (var item in properties.Select((p, i) => new { Index = i, PropertyInfo = p }).Where(i => i.PropertyInfo.PropertyType == typeof(DateTime)))
                {
                    table.Columns[item.Index].DataCellStyleName = "Date";
                }

                r1.AutoFitColumns();
            }
            BinaryWriter bw = new BinaryWriter(streamWriter);
            bw.Write(pck.GetAsByteArray());
        }
    }
}
