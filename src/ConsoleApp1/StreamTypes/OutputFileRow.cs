using Paillave.Etl.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ConsoleApp1.StreamTypes
{
    public class OutputFileRow
    {
        public string FileName { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class OutputFileRowMapper : ColumnNameFlatFileDescriptor<OutputFileRow>
    {
        public OutputFileRowMapper()
        {
            this.MapColumnToProperty("Id", i => i.Id);
            this.MapColumnToProperty("Name", i => i.Name);
            this.MapColumnToProperty("FileName", i => i.FileName);
            this.IsFieldDelimited(',');
        }
    }
}
