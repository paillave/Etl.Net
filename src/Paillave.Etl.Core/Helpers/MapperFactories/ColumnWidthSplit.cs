using System.Linq;
using System.Collections.Generic;

namespace Paillave.Etl.Helpers.MapperFactories
{
    public class ColumnWidthSplit
    {
        private readonly int[] _columnSize;

        public ColumnWidthSplit(params int[] columnSize)
        {
            this._columnSize = columnSize;
        }

        private IEnumerable<string> InternalParseFixedColumnLine(string line)
        {
            foreach (var item in _columnSize)
            {
                yield return line.Substring(0, item);
                line = line.Substring(item);
            }
        }

        public IList<string> ParseFixedColumn(string line)
        {
            return InternalParseFixedColumnLine(line).ToList();
        }
    }
}
