using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using static UglyToad.PdfPig.Core.PdfSubpath;

namespace Paillave.Pdf
{
    public class GridExtractor
    {
        private readonly double _proximity;
        public GridExtractor(double proximity = 3) => _proximity = proximity;
        private IEnumerable<GridLine> ExtractLines(Page page)
        {
            foreach (var path in page.ExperimentalAccess.Paths)
            {
                var color = path.FillColor ?? path.StrokeColor;
                if (!path.IsFilled && !path.IsStroked) continue;
                if (color == null || color.ToRGBValues() == (1, 1, 1)) continue;
                foreach (var subpath in path)
                {
                    if (!(subpath.Commands[0] is Move first)) continue;
                    if (subpath.Commands.Any(c => c is BezierCurve)) continue;

                    PdfPoint? start_pos = first.Location;
                    PdfPoint? last_move = start_pos;
                    PdfPoint? end_pos = null;

                    foreach (var command in subpath.Commands)
                    {
                        switch (command)
                        {
                            case Line linePath:
                                end_pos = linePath.To;
                                if (start_pos == null) break;
                                yield return new GridLine(start_pos.Value, linePath.To, _proximity, color);
                                break;
                            case Move move:
                                start_pos = move.Location;
                                end_pos = start_pos;
                                break;
                            case Close:
                                if (start_pos == null || end_pos == null) break;
                                yield return new GridLine(last_move.Value, end_pos.Value, _proximity, color);
                                break;
                        }
                        start_pos = end_pos;
                    }
                }
            }
        }
        public GridExtraction Extract(Page page, bool debug = false)
        {
            var rulings = ExtractLines(page);
            var grids = GetGrids(rulings).ToList();
            foreach (var grid in grids) grid.Build();

            grids = ExcludeInnerBounds(grids).Where(i => i.LinesX.Count > 2 && i.LinesY.Count > 2).ToList();

            var outOfScopeHorizontalLines = rulings
                                .Where(r => r.IsHorizontal && !grids.Any(g => g.Top + _proximity >= r.Top && g.Bottom - _proximity <= r.Top))
                                .Select(l => new HorizontalLine(l.Top, l.Left, l.Right)).ToList();

#if DEBUG
            if (debug && grids.Any())
            {
                var svg = new SvgBuilder((int)page.Width, (int)page.Height, page.Number, "RelatedLines", true);
                foreach (var item in grids) item.DrawRelatedLines(svg);
                svg.Show();
                // var svg2 = new SvgBuilder((int)page.Width, (int)page.Height, page.Number, true);
                // foreach (var item in grids) item.DrawSimplifiedRelatedLines(svg2);
                // svg2.Show("SimplifiedRelatedLines");
                // var svg3 = new SvgBuilder((int)page.Width, (int)page.Height, page.Number, true);
                // foreach (var item in grids) item.DrawCellLines(svg3);
                // svg3.Show("CellLines");
                var svg4 = new SvgBuilder((int)page.Width, (int)page.Height, page.Number, "Grid", true);
                foreach (var item in grids) item.DrawGrid(svg4);
                svg4.Show();
            }
#endif
            return new GridExtraction
            {
                Grids = grids,
                OutOfScopeHorizontalLines = outOfScopeHorizontalLines
            };
        }
        private IEnumerable<Grid> GetGrids(IEnumerable<GridLine> inputLines)
        {
            var grids = new List<Grid>();
            var lines = inputLines.Where(i => i.IsHorizontal || i.IsVertical).ToList();
            var processedLines = new HashSet<GridLine>();
            foreach (var line in lines)
                if (!processedLines.Contains(line))
                    yield return GetGrids(line, lines, processedLines);
        }
        private Grid GetGrids(GridLine line, List<GridLine> lines, HashSet<GridLine> processedLines, Grid grid = null)
        {
            if (grid == null) grid = new Grid(_proximity);
            grid.AddLine(line);
            processedLines.Add(line);
            var intersects = lines.Where(l => !processedLines.Contains(l) && Intersects(l, line)).ToList();
            foreach (var intersect in intersects) GetGrids(intersect, lines, processedLines, grid);
            return grid;
        }
        private bool Intersects(GridLine line1, GridLine line2)
        {
            if (line1.IsHorizontal == line2.IsHorizontal) return false;
            if (line1.IsHorizontal) return line1.Top <= (line2.Top + _proximity) && line1.Top >= (line2.Bottom - _proximity)
                && line2.Left >= (line1.Left - _proximity) && line2.Left <= (line1.Right + _proximity);
            return line2.Top <= (line1.Top + _proximity) && line2.Top >= (line1.Bottom - _proximity)
                && line1.Left >= (line2.Left - _proximity) && line1.Left <= (line2.Right + _proximity);
        }
        private List<B> ExcludeInnerBounds<B>(List<B> bounds) where B : IBounds
        {
            var outerBounds = new List<B>();
            foreach (var bound in bounds)
            {
                if (outerBounds.FindIndex(i => Contains(i, bound)) < 0)
                {
                    outerBounds.RemoveAll(i => Contains(bound, i));
                    outerBounds.Add(bound);
                }
            }
            return outerBounds;
        }
        private bool Contains(IBounds outerBound, IBounds innerBound)
            => outerBound.Left <= innerBound.Left && outerBound.Right >= innerBound.Right && outerBound.Top >= innerBound.Top && outerBound.Bottom <= innerBound.Bottom;
    }
    public class GridExtraction
    {
        public List<Grid> Grids { get; set; }
        // useful to detect is a text is underlined
        public List<HorizontalLine> OutOfScopeHorizontalLines { get; set; }
    }
    public class HorizontalLine
    {
        public HorizontalLine(double y, double left, double right) => (Y, Left, Right) = (y, left, right);
        public double Y { get; }
        public double Left { get; }
        public double Right { get; }
    }
}
