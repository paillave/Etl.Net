using Paillave.Etl.Core.StreamNodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ConsoleApp1.StreamTypes
{
    public class Class2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
    }

    public class Class2Mapper : ColumnNameFlatFileDescriptor<Class2>
    {
        public Class2Mapper()
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
            this.MapColumnToProperty("Label", i => i.Name);
            this.MapFileNameToProperty(i => i.FileName);
            this.IsFieldDelimited('\t');
        }
    }
}
