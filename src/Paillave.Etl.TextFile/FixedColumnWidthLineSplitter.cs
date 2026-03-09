using System.Linq;
using System.Collections.Generic;
using System;

namespace Paillave.Etl.TextFile;

public class FixedColumnWidthLineSplitter(params int[] columnSize) : ILineSplitter
{
    private readonly int[] _columnSize = columnSize;
    private readonly string _joinStringFormat = string.Join("", columnSize.Select((s, idx) => $"{{{idx},{s}}}"));

    private IEnumerable<string> InternalParseFixedColumnLine(string line)
    {
        foreach (var item in _columnSize)
        {
            yield return line.Substring(0, Math.Abs(item));
            line = line.Substring(Math.Abs(item));
        }
    }

    public IList<string> Split(string line)
    {
        return InternalParseFixedColumnLine(line).ToList();
    }

    public string Join(IEnumerable<string> line)
    {
        return string.Format(_joinStringFormat, line.ToArray());
    }
}
