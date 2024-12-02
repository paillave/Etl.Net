using System;
using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;

namespace Paillave.Pdf
{
    public class SimpleLinesMethod : ExtractMethod
    {
        public SimpleLinesMethod(WordExtractionType wordExtractionType, Areas areas) : base(wordExtractionType) { }
        private class ProcessingLine
        {
            private readonly List<Word> _words = new List<Word>();
            private readonly List<double> _heights = new List<double>();
            private double _lastTop;
            private double _lastBottom;
            public HashSet<string> Areas { get; }
            public ProcessingLine(Word word, double height, HashSet<string> areas)
            {
                Areas = areas;
                (_lastTop, _lastBottom) = (word.BoundingBox.Top, Math.Min(word.BoundingBox.Bottom, word.BoundingBox.Top - height));
                _heights.Add(height);
                _words.Add(word);
            }
            public bool IsSameLine(Word word, double height, HashSet<string> areas)
            {
                if ((areas.Any() && !this.Areas.Any()) || (!areas.Any() && this.Areas.Any())) return false;
                if (areas.Any() && this.Areas.Any() && !areas.Intersect(Areas).Any()) return false;
                var bottom = Math.Min(word.BoundingBox.Bottom, word.BoundingBox.Top - height);
                var middle = (word.BoundingBox.Top + bottom) / 2;
                var lastMiddle = (_lastTop + _lastBottom) / 2;
                return (middle < _lastTop && middle > _lastBottom) || (lastMiddle < word.BoundingBox.Top && lastMiddle > bottom);
            }
            // public bool IsSameLine(Word word, double height, string area)
            // {
            //     if (area != Area) return false;
            //     var bottom = Math.Min(word.BoundingBox.Bottom, word.BoundingBox.Top - height);
            //     var middle = (word.BoundingBox.Top + bottom) / 2;
            //     var lastMiddle = (_lastTop + _lastBottom) / 2;
            //     return (middle < _lastTop && middle > _lastBottom) || (lastMiddle < word.BoundingBox.Top && lastMiddle > bottom);
            // }
            public void AddWord(Word word, double height)
            {
                _lastTop = word.BoundingBox.Top;
                _lastBottom = Math.Min(word.BoundingBox.Bottom, word.BoundingBox.Top - height);
                _heights.Add(height);
                _words.Add(word);
            }
            public IEnumerable<TextLine> GetTextLines()
            {
                var maxSpace = _heights.Average();
                Word previousWord = null;
                List<Word> currentBlock = new List<Word>();
                foreach (var word in _words.OrderBy(w => w.BoundingBox.Left).ThenByDescending(w => w.BoundingBox.Top).Select(i => CloneWord(i)).ToList())
                {
                    if (previousWord != null)
                    {
                        if (previousWord.BoundingBox.Right + maxSpace < word.BoundingBox.Left)
                        {
                            if (currentBlock.Count > 0)
                                yield return new TextLine(currentBlock);
                            currentBlock = new List<Word>();
                        }
                        currentBlock.Add(word);
                        previousWord = word;
                    }
                    else
                    {
                        previousWord = word;
                        currentBlock.Add(word);
                    }
                }
                if (currentBlock.Count > 0)
                {
                    yield return new TextLine(currentBlock);
                }
            }
            private Word CloneWord(Word word)
            {
                return word;
                // var ret = new Word(word.Letters.Select(l => CloneLetter(l)).ToList());
                // return ret;
            }
            private Letter CloneLetter(Letter letter)
                => new Letter(
                    letter.Value,
                    new UglyToad.PdfPig.Core.PdfRectangle(
                        letter.GlyphRectangle.Left,
                        letter.GlyphRectangle.Bottom,
                        letter.GlyphRectangle.Right,
                        letter.GlyphRectangle.Height > 1 ? letter.GlyphRectangle.Top : letter.GlyphRectangle.Bottom + letter.PointSize),
                    letter.StartBaseLine,
                    letter.EndBaseLine,
                    letter.Width,
                    letter.FontSize,
                    letter.Font,
                    UglyToad.PdfPig.Core.TextRenderingMode.Fill,
                    letter.Color,
                    letter.Color,
                    letter.PointSize,
                    letter.TextSequence);
        }
        private class ProcessingLines
        {
            private readonly List<ProcessingLine> _processingLines = new List<ProcessingLine>();
            private readonly Areas _areas;
            public ProcessingLines(Areas areas) => _areas = areas;
            public void AddWord(Word word, double height, Page page)
            {
                if (string.IsNullOrWhiteSpace(word.Text)) return;
                var areas = _areas.GetAreaCodes(word.BoundingBox, page);
                var processingLine = _processingLines.FirstOrDefault(i => i.IsSameLine(word, height, areas));
                if (processingLine == null)
                {
                    processingLine = new ProcessingLine(word, height, areas);
                    _processingLines.Add(processingLine);
                }
                else
                {
                    processingLine.AddWord(word, height);
                }
            }
            public List<(TextBlock, HashSet<string>)> GetTextLines() => _processingLines.Select(i =>
            {
                var lines = i.GetTextLines().ToList();
                return (new TextBlock(lines, "\t"), i.Areas);
            }).ToList();
        }
        internal override List<ProcessedBlock> ExtractTextBlocks(Page page)
        {
            var processingLines = new ProcessingLines(this.Areas);
            var words = page.GetWords(this.WordExtractor).Where(i => !string.IsNullOrWhiteSpace(i.Text))
                .Where(i => i.BoundingBox.Rotation < 10 || i.BoundingBox.Rotation > 350)
                .OrderBy(i => i.BoundingBox.Left).ThenByDescending(i => i.BoundingBox.Top).ToList();
            foreach (var word in words)
            {
                var height = word.BoundingBox.Height;
                height = Math.Max(height, word.Letters[0].PointSize);
                processingLines.AddWord(word, height, page);
            }
            return processingLines.GetTextLines().OrderByDescending(i => i.Item1.BoundingBox.Top).Select(i => new ProcessedBlock { KeepTogether = true, TextBlock = i.Item1, AreaCodes = i.Item2 }).ToList();
        }
    }
}