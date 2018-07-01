using Paillave.Etl.Core.Helpers.MapperFactories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.MapperFactories
{
    public static partial class Mappers
    {
        public static Func<string, IEnumerable<string>> FixedColumnLineSplitter(params int[] columnSize)
        {
            return new ColumnWidthSplit(columnSize).ParseFixedColumn;
        }
    }
}
