using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Paillave.Etl.Core.Helpers
{
    public class FixedColumnsLineSplitter : ILineSplitter
    {
        private int[] _columnSize;
        public FixedColumnsLineSplitter(params int[] columnSize)
        {
            this._columnSize = columnSize;
        }

        public string[] Split(string line) => SplitIntern(line).ToArray();

        private IEnumerable<string> SplitIntern(string line)
        {
            foreach (var item in this._columnSize)
            {
                yield return line.Substring(0, item);
                line = line.Substring(item);
            }
        }
    }
}
