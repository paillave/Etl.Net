using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.Outline;

namespace Paillave.Pdf;

public class TextTemplate
{
    private readonly List<ITextTemplateCheck> _checks = new();
    public TextTemplate Pattern(string pattern)
    {
        _checks.Add(new PatternTextTemplateCheck(pattern));
        return this;
    }
    public TextTemplate Bold(bool bold = true)
    {
        _checks.Add(new BoldTextTemplateCheck(bold));
        return this;
    }
    public TextTemplate Italic(bool italic = true)
    {
        _checks.Add(new ItalicTextTemplateCheck(italic));
        return this;
    }
    public TextTemplate Underlined(bool underlined = true)
    {
        _checks.Add(new UnderlinedTextTemplateCheck(underlined));
        return this;
    }
    public TextTemplate Size(int size)
    {
        _checks.Add(new SizeTextTemplateCheck(size));
        return this;
    }
    public TextTemplate Color(int r, int g, int b)
    {
        _checks.Add(new ColorTextTemplateCheck(r, g, b));
        return this;
    }
    public TextTemplate Center() => Alignment(AlignmentType.Center);
    public TextTemplate Right() => Alignment(AlignmentType.Right);
    public TextTemplate Left() => Alignment(AlignmentType.Left);
    public TextTemplate Alignment(AlignmentType alignment)
    {
        _checks.Add(new AlignmentTextTemplateCheck(alignment));
        return this;
    }
    public TextTemplate Offset(double offset)
    {
        _checks.Add(new OffsetTextTemplateCheck(offset));
        return this;
    }
    public TextTemplate Bookmark(List<List<DocumentBookmarkNode>> bookmarks)
    {
        _checks.Add(new BookmarkTextTemplateCheck(bookmarks));
        return this;
    }
    internal bool Check(TextLine textLine, Page page, List<HorizontalLine> lines)
        => _checks.TrueForAll(i => i.Check(textLine, page, lines));

    private interface ITextTemplateCheck
    {
        bool Check(TextLine textLine, Page page, List<HorizontalLine> lines);
    }
    private class PatternTextTemplateCheck(string pattern) : ITextTemplateCheck
    {
        private readonly Regex _regex = new(pattern, RegexOptions.Compiled);

        public bool Check(TextLine textLine, Page page, List<HorizontalLine> lines) => _regex.IsMatch(textLine.Text);
    }
    private class BoldTextTemplateCheck(bool bold) : ITextTemplateCheck
    {
        private readonly bool _bold = bold;

        public bool Check(TextLine textLine, Page page, List<HorizontalLine> lines)
            => _bold == textLine.Words.Any(word => word.Letters.Where(letter => letter.Value != " ").Any(letter => letter.Font.IsBold || letter.Font.Weight > 500));
    }
    private class UnderlinedTextTemplateCheck(bool underlined) : ITextTemplateCheck
    {
        private readonly bool _underlined = underlined;

        public bool Check(TextLine textLine, Page page, List<HorizontalLine> lines)
        {
            return _underlined == lines.Any(l => l.Left < textLine.BoundingBox.Right && l.Right > textLine.BoundingBox.Left && l.Y < textLine.BoundingBox.Bottom && l.Y > textLine.BoundingBox.Bottom - textLine.BoundingBox.Height);
        }
    }
    private class ItalicTextTemplateCheck(bool italic) : ITextTemplateCheck
    {
        private readonly bool _italic = italic;

        public bool Check(TextLine textLine, Page page, List<HorizontalLine> lines)
            => _italic == textLine.Words.Any(word => word.Letters.Where(letter => letter.Value != " ").Any(letter => letter.Font.IsItalic));
    }
    private class SizeTextTemplateCheck(int size) : ITextTemplateCheck
    {
        private readonly int _size = size;

        public bool Check(TextLine textLine, Page page, List<HorizontalLine> lines)
            => textLine.Words.Any(word => word.Letters.Where(letter => letter.Value != " ").Any(letter => System.Math.Round(letter.PointSize) == _size));
    }
    private class ColorTextTemplateCheck : ITextTemplateCheck
    {
        private readonly int _r;
        private readonly int _g;
        private readonly int _b;
        public ColorTextTemplateCheck(int r, int g, int b) => (_r, _g, _b) = (r, g, b);
        public bool Check(TextLine textLine, Page page, List<HorizontalLine> lines)
            => textLine.Words.Any(word => word.Letters.Where(letter => letter.Value != " ").Any(letter =>
            {
                var (r, g, b) = letter.Color.ToRGBValues();
                return System.Math.Round(255 * r) == _r && System.Math.Round(g * 255) == _g && System.Math.Round(b * 255) == _b;
            }));
    }
    private class AlignmentTextTemplateCheck(AlignmentType alignment) : ITextTemplateCheck
    {
        private readonly AlignmentType _alignment = alignment;

        public bool Check(TextLine textLine, Page page, List<HorizontalLine> lines)
        {
            switch (_alignment)
            {
                case AlignmentType.Left: return textLine.BoundingBox.Left < page.Width * 0.2 && textLine.BoundingBox.Left < (page.Width - textLine.BoundingBox.Right);
                case AlignmentType.Center: return textLine.BoundingBox.Centroid.X > (page.Width * 2 / 5) && textLine.BoundingBox.Centroid.X < (page.Width * 3 / 5);
                case AlignmentType.Right: return textLine.BoundingBox.Right > page.Width * 0.8 && textLine.BoundingBox.Left > (page.Width - textLine.BoundingBox.Right);
            }
            return true;
        }
    }
    private class OffsetTextTemplateCheck(double offset) : ITextTemplateCheck
    {
        private readonly double _offset = offset;

        public bool Check(TextLine textLine, Page page, List<HorizontalLine> lines)
        {
            var offset = page.Width * _offset / 100;
            var left = offset - 20;
            var right = offset + 20;
            return textLine.BoundingBox.Left > left && textLine.BoundingBox.Left < right;
        }
    }
    private class BookmarkTextTemplateCheck(List<List<DocumentBookmarkNode>> bookmarks) : ITextTemplateCheck
    {
        private readonly List<List<DocumentBookmarkNode>> _bookmarks = bookmarks;

        public bool Check(TextLine textLine, Page page, List<HorizontalLine> lines)
            => GetMatchingBookmark((decimal)textLine.BoundingBox.Top, (decimal)textLine.BoundingBox.Bottom, page.Number) != null;
        private List<DocumentBookmarkNode>? GetMatchingBookmark(decimal top, decimal bottom, int pageNumber)
        {
            if (_bookmarks == null) return null;
            var heigh = top - bottom;
            return _bookmarks.FirstOrDefault(i =>
            {
                var bBottom = i[0].Destination.Coordinates.Bottom ?? i[0].Destination.Coordinates.Top??0 - (double)heigh;
                var bTop = i[0].Destination.Coordinates.Top ?? i[0].Destination.Coordinates.Bottom??0 + (double)heigh;
                return i[0].PageNumber == pageNumber && bBottom <= (double)top && bTop >= (double)bottom;
            });
        }
    }
}
public enum AlignmentType
{
    Left,
    Right,
    Center
}
