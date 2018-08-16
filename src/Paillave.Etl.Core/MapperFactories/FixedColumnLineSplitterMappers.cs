using Paillave.Etl.Helpers.MapperFactories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.MapperFactories
{
    public static partial class Mappers
    {
        public static Func<string, IList<string>> FixedColumnLineSplitter(params int[] columnSize)
        {
            return new ColumnWidthSplit(columnSize).ParseFixedColumn;
        }
    }
}
