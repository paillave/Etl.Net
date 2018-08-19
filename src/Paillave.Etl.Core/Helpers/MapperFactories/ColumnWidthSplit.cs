using System.Linq;
using System.Collections.Generic;
using System;

namespace Paillave.Etl.Helpers.MapperFactories
{
    public class ColumnWidthSplit
    {
        private readonly int[] _columnSize;
        private readonly string _joinStringFormat;

        public ColumnWidthSplit(params int[] columnSize)
        {
            _joinStringFormat = string.Join("", columnSize.Select((s, idx) => $"{{{idx},{s}}}"));
            this._columnSize = columnSize;
        }

        private IEnumerable<string> InternalParseFixedColumnLine(string line)
        {
            foreach (var item in _columnSize)
            {
                yield return line.Substring(0, Math.Abs(item));
                line = line.Substring(item);
            }
        }

        public IList<string> ParseFixedColumn(string line)
        {
            return InternalParseFixedColumnLine(line).ToList();
        }

        public string JoinFixedColumn(IEnumerable<string> line)
        {
            return string.Format(_joinStringFormat, line.ToArray());
        }
    }
}
