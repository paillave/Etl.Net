using System;
using System.Collections.Generic;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.IO;
using System.Reflection;
using System.Linq;

namespace Paillave.Etl.ExcelFile.Core
{
    public static class IEnumerableExcelEx
    {
        public static void WriteExcelListInStream<TIn>(this IList<TIn> value, ExcelFileDefinition<TIn> mapping, Stream streamWriter)
        {
            ExcelPackage pck = WriteExcelList(value, mapping.GetExcelReader());
            BinaryWriter bw = new BinaryWriter(streamWriter);
            bw.Write(pck.GetAsByteArray());
        }
        public static void WriteExcelListInStream<TIn>(this IList<TIn> value, Stream streamWriter)
        {
            ExcelPackage pck = WriteExcelList(value, null);
            BinaryWriter bw = new BinaryWriter(streamWriter);
            bw.Write(pck.GetAsByteArray());
        }
        public static void WriteExcelListInStream<TIn>(this IList<TIn> value, ExcelFileReader excelReader, Stream streamWriter)
        {
            ExcelPackage pck = WriteExcelList(value, excelReader);
            BinaryWriter bw = new BinaryWriter(streamWriter);
            bw.Write(pck.GetAsByteArray());
        }

        private static ExcelPackage WriteExcelList<TIn>(IList<TIn> value, ExcelFileReader excelReader)
        {
            var pck = new ExcelPackage();
            var style = pck.Workbook.Styles.CreateNamedStyle("Date");
            style.Style.Numberformat.Format = "dd/MM/yyyy";
            var wsList = pck.Workbook.Worksheets.Add("Sheet1");
            if (excelReader != null)
            {
                var excelColumns = excelReader.PropertySetters.OrderBy(i => i.Key).Select(i => i.Value).ToArray();
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
            return pck;
        }
    }
}