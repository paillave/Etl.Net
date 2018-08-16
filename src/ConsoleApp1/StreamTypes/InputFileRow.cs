using Paillave.Etl.Helpers;
using Paillave.Etl.StreamNodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ConsoleApp1.StreamTypes
{
    public class InputFileRow
    {
        public int Id { get; set; }
        public DateTime Col1 { get; set; }
        public decimal Col2 { get; set; }
        public int Col3 { get; set; }
        public string Col4 { get; set; }
        public int TypeId { get; set; }
        public string FileName { get; set; }
    }

    public class InputFileRowMapper : ColumnNameFlatFileDescriptor<InputFileRow>
    {
        public InputFileRowMapper()
        {
            CultureInfo ci = CultureInfo.CreateSpecificCulture("en-GB");
            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            ci.DateTimeFormat.FullDateTimePattern = "yyyy-MM-dd HH:mm:ss";
            ci.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            ci.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";

            ci.NumberFormat.NumberDecimalSeparator = ",";
            ci.NumberFormat.CurrencyDecimalSeparator = ",";
            ci.NumberFormat.PercentDecimalSeparator = ",";

            this.WithCultureInfo(ci);
            this.MapColumnToProperty("#", i => i.Id);
            this.MapColumnToProperty("DateTime", i => i.Col1);
            this.MapColumnToProperty("Value", i => i.Col2);
            this.MapColumnToProperty("Rank", i => i.Col3);
            this.MapColumnToProperty("Comment", i => i.Col4);
            this.MapColumnToProperty("TypeId", i => i.TypeId);
            //this.MapFileNameToProperty(i => i.FileName);
            this.IsFieldDelimited('\t');
        }
    }
}
