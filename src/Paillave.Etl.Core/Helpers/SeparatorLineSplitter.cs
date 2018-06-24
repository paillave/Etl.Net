using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.Helpers
{
    public class SeparatorLineSplitter : ILineSplitter
    {
        private string _separator;
        public SeparatorLineSplitter(string separator)
        {
            this._separator = separator;
        }
        public string[] Split(string line)
        {
            return line.Split(new[] { this._separator }, StringSplitOptions.None);
        }
    }
}
