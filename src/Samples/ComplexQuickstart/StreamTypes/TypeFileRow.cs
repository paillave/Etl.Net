using System.Globalization;
using Paillave.Etl.TextFile.Core;

namespace ComplexQuickstart.StreamTypes
{
    public class TypeFileRow
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string FileName { get; set; }
    }

    public class TypeFileRowMapper : FileDefinition<TypeFileRow>
    {
        public TypeFileRowMapper()
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
            this.MapColumnToProperty("Category", i => i.Category);
            this.IsColumnSeparated('\t');
        }
    }
}
