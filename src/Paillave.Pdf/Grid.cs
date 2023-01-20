using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Content;

namespace Paillave.Pdf
{
    public class Grid : IBounds
    {
        private readonly double _proximity;
        public Grid(double proximity = 0) => _proximity = proximity;
        public double Top { get; private set; }
        public double Bottom { get; private set; }
        public double Left { get; private set; }
        public double Right { get; private set; }
        public List<double> LinesX { get; private set; } = new List<double>();
        public List<double> LinesY { get; private set; } = new List<double>();
        private List<GridLine> _relatedLines = new List<GridLine>();
        private List<GridLine> _simplifiedRelatedLines = new List<GridLine>();
        private List<GridLine> _cellLines = new List<GridLine>();
        private List<Word>[][] _content;
        internal void Build()
        {
            BuildLines();
            if (LinesY.Count > 1 && LinesX.Count > 1)
                _content = Enumerable.Range(0, LinesY.Count - 1).Select(i => Enumerable.Range(0, LinesX.Count - 1).Select(j => new List<Word>()).ToArray()).ToArray();
            else
                _content = new[] { new[] { new List<Word>() } };
        }
        private void BuildLines()
        {
            _simplifiedRelatedLines = SimplifyLines(_relatedLines);
            if (_simplifiedRelatedLines.Count > 0)
            {
                this.Bottom = _simplifiedRelatedLines.Min(l => l.Bottom);
                this.Top = _simplifiedRelatedLines.Max(l => l.Top);
                this.Left = _simplifiedRelatedLines.Min(l => l.Left);
                this.Right = _simplifiedRelatedLines.Max(l => l.Right);
            }
            _cellLines = CleanUpInnerLines(_simplifiedRelatedLines);
            this.LinesX = _cellLines.Where(l => l.IsVertical).Select(l => l.Left).Distinct().OrderBy(i => i).ToList();
            this.LinesY = _cellLines.Where(l => l.IsHorizontal).Select(l => l.Top).Distinct().OrderBy(i => i).ToList();
        }
        private List<GridLine> CleanUpInnerLines(List<GridLine> lines)
        {
            var top = this.Top - _proximity;
            var bottom = this.Bottom + _proximity;
            var right = this.Right - _proximity;
            var left = this.Left + _proximity;
            var linesX = lines.Where(l => l.IsVertical && l.Top >= top && l.Bottom <= bottom).Select(l => l.Left).Distinct().OrderBy(i => i).ToList();
            var linesY = lines.Where(l => l.IsHorizontal && l.Left <= left && l.Right >= right).Select(l => l.Top).Distinct().OrderBy(i => i).ToList();
            var linesToRemove = linesX.Pair().SelectMany(x => linesY.Pair().Select(y => new CellBounds(y.Item2, y.Item1, x.Item1, x.Item2))).SelectMany(c => lines.Where(l => Contains(c, l))).ToHashSet();

            return lines.Where(l => !linesToRemove.Contains(l)).ToList();
        }
        private class CellBounds : IBounds
        {
            public CellBounds(double top, double bottom, double left, double right)
                => (Top, Bottom, Left, Right) = (top, bottom, left, right);
            public double Top { get; }
            public double Bottom { get; }
            public double Left { get; }
            public double Right { get; }
        }
        private bool Contains(IBounds outer, IBounds inner)
            => outer.Left - _proximity < inner.Left && outer.Right + _proximity > inner.Right
                && outer.Bottom - _proximity < inner.Bottom && outer.Top + _proximity > inner.Top;
        private List<GridLine> SimplifyLines(List<GridLine> lines)
        {
            var comparer = new ApproximateEqualityComparer(_proximity);
            var horizontalLines = lines
                .Where(line => line.IsHorizontal && !line.IsVertical)
                .GroupBy(line => line.Top, comparer)
                .SelectMany(lines => SimplifySegments(lines.OrderBy(line => line.Left).Select(line => (line.Left, line.Right))).Select(line => new GridLine(
                        new UglyToad.PdfPig.Core.PdfPoint(line.Item1, lines.Key),
                        new UglyToad.PdfPig.Core.PdfPoint(line.Item2, lines.Key),
                        _proximity,
                        null))).ToList();
            var verticalLines = lines
                .Where(line => line.IsVertical && !line.IsHorizontal)
                .GroupBy(line => line.Left, comparer)
                .SelectMany(lines => SimplifySegments(lines.OrderBy(line => line.Bottom).Select(line => (line.Bottom, line.Top))).Select(line => new GridLine(
                        new UglyToad.PdfPig.Core.PdfPoint(lines.Key, line.Item1),
                        new UglyToad.PdfPig.Core.PdfPoint(lines.Key, line.Item2),
                        _proximity,
                        null))).ToList();
            return horizontalLines.Union(verticalLines).ToList();
        }
        private IEnumerable<(double, double)> SimplifySegments(IEnumerable<(double, double)> segments)
        {
            double? currentEnd = null;
            double? currentStart = null;
            foreach (var (start, end) in segments)
            {
                if (currentStart == null) currentStart = start;
                if (currentEnd != null && start > currentEnd.Value + _proximity)
                {
                    if (currentStart.Value <= currentEnd.Value + _proximity)
                        yield return (currentStart.Value, currentEnd.Value);
                    currentStart = start;
                }
                if (currentEnd == null) currentEnd = end;
                else currentEnd = Math.Max(end, currentEnd.Value);
            }
            if (currentStart.Value <= currentEnd.Value + _proximity)
                yield return (currentStart.Value, currentEnd.Value);
        }
        private static IEnumerable<string> ReflowText(IEnumerable<Word> words) => new LinesOfWords(words).GetLines();
        public bool TryAddWord(Word word)
        {
            if (this.TryGetRelatedCell(word, out var loc))
            {
                var rowNumber = this.LinesY.Count - 2 - loc.row;
                _content[rowNumber][loc.column].Add(word);
                return true;
            }
            return false;
        }
        public List<List<List<string>>> GetDataContent()
            => _content.Select(row => row.Select(cell => ReflowText(cell).ToList()).ToList()).ToList();
        public void AddLine(GridLine gridLine) => _relatedLines.Add(gridLine);
        public bool TryGetRelatedCell(Word textLine, out (int row, int column) cell)
        {
            var centerX = textLine.BoundingBox.Centroid.X;
            var centerY = textLine.BoundingBox.Centroid.Y;
            // var centerX = (textLine.BoundingBox.Left + textLine.BoundingBox.Right) / 2;
            // var centerY = (textLine.BoundingBox.Top + textLine.BoundingBox.Bottom) / 2;
            cell = default;
            if (centerX < this.Left || centerX > this.Right) return false;
            if (centerY < this.Bottom || centerY > this.Top) return false;
            cell = (
                this.LinesY.GetPosition(centerY).Item1,
                this.LinesX.GetPosition(centerX).Item1
            );
            return true;
        }
        public void DrawGrid(SvgBuilder builder)
        {
            foreach (var x in LinesX) builder.VerticalLine(x, this.Top, this.Bottom);
            foreach (var y in LinesY) builder.HorizontalLine(this.Left, y, this.Right);
            builder.Rectangle(this.Left, this.Top, this.Right, this.Bottom, "orange");
        }
        public void DrawCellLines(SvgBuilder builder)
        {
            foreach (var x in _cellLines)
                builder.Line(x.Left, x.Top, x.Right, x.Bottom);
            builder.Rectangle(this.Left, this.Top, this.Right, this.Bottom, "orange");
        }
        public void DrawRelatedLines(SvgBuilder builder)
        {
            foreach (var x in _relatedLines)
                builder.Line(x.Left, x.Top, x.Right, x.Bottom);
            builder.Rectangle(this.Left, this.Top, this.Right, this.Bottom, "orange");
        }
        public void DrawSimplifiedRelatedLines(SvgBuilder builder)
        {
            foreach (var x in _simplifiedRelatedLines)
                builder.Line(x.Left, x.Top, x.Right, x.Bottom);
            builder.Rectangle(this.Left, this.Top, this.Right, this.Bottom, "orange");
        }
    }
}
