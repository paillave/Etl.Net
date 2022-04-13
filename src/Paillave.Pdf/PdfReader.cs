using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Util;

namespace Paillave.Pdf
{
    public enum WordExtractionType
    {
        Default = 0,
        NearestNeighbour = 1
    }
    internal class ProcessedBlock
    {
        public TextBlock TextBlock { get; set; }
        public bool KeepTogether { get; set; }
        public string AreaCode { get; set; }
    }
    public class Areas : Dictionary<string, PdfZone>
    {
        public string GetAreaCode(UglyToad.PdfPig.Core.PdfRectangle rectangle, Page page)
                => this.FirstOrDefault(a =>
                {
                    if (a.Value.PageNumber != null && page.Number != a.Value.PageNumber.Value)
                        return false;
                    var percentageRectangle = new UglyToad.PdfPig.Core.PdfRectangle(rectangle.Left / page.Width, 1 - rectangle.Top / page.Height, rectangle.Right / page.Width, 1 - rectangle.Bottom / page.Height);
                    return a.Value.IsInZone(percentageRectangle);
                }).Key;
    }
    public abstract class ExtractMethod
    {
        internal Areas Areas { get; set; }
        public ExtractMethod(WordExtractionType wordExtractionType)
        {
            switch (wordExtractionType)
            {
                case WordExtractionType.Default:
                    this.WordExtractor = DefaultWordExtractor.Instance;
                    break;
                case WordExtractionType.NearestNeighbour:
                    this.WordExtractor = NearestNeighbourWordExtractor.Instance;
                    break;
            }
        }
        public static ExtractMethod RecursiveXY(WordExtractionType wordExtractionType = WordExtractionType.NearestNeighbour, Areas areas = null) => new RecursiveXYSegmentMethod(wordExtractionType, areas ?? new Areas());
        public static ExtractMethod Docstrum(WordExtractionType wordExtractionType = WordExtractionType.NearestNeighbour, Areas areas = null) => new DocstrumSegmentMethod(wordExtractionType, areas ?? new Areas());
        public static ExtractMethod SimpleLines(WordExtractionType wordExtractionType = WordExtractionType.NearestNeighbour, Areas areas = null) => new SimpleLinesMethod(wordExtractionType, areas ?? new Areas());
        protected virtual IEnumerable<TextBlock> GetTextGroups(Page page, IEnumerable<Word> words)
        {
            yield break;
        }
        internal virtual List<ProcessedBlock> ExtractTextBlocks(Page page)
        {
            return page
                .GetWords(this.WordExtractor)
                .Where(i => i.BoundingBox.Rotation < 10 || i.BoundingBox.Rotation > 350)
                .Select(word => new
                {
                    Area = this.Areas.GetAreaCode(word.BoundingBox, page),
                    Word = word
                })
                .GroupBy(i => i.Area)
                .SelectMany(i => GetTextGroups(page, i.Select(i => i.Word).ToList()).Select(j => new ProcessedBlock { KeepTogether = false, AreaCode = i.Key, TextBlock = j }))
                .OrderByDescending(i=>i.TextBlock.BoundingBox.Top)
                .ToList();



            // var words = page
            //     .GetWords(this.WordExtractor)
            //     .Where(i => i.BoundingBox.Rotation < 10 || i.BoundingBox.Rotation > 350)
            //     .ToList();
            // var blocks = GetTextGroups(page, words)
            //     .ToList();
            // return blocks.Select(i => new ProcessedBlock { KeepTogether = false, TextBlock = i }).ToList();//.SelectMany(i => i.TextLines).ToList();
        }
        protected IWordExtractor WordExtractor { get; }
    }
    public class SimpleLinesMethod : ExtractMethod
    {
        public SimpleLinesMethod(WordExtractionType wordExtractionType, Areas areas) : base(wordExtractionType) { }
        private class ProcessingLine
        {
            private readonly List<Word> _words = new List<Word>();
            private readonly List<double> _heights = new List<double>();
            private double _lastTop;
            private double _lastBottom;
            public string Area { get; }
            public ProcessingLine(Word word, double height, string area)
            {
                Area = area;
                (_lastTop, _lastBottom) = (word.BoundingBox.Top, Math.Min(word.BoundingBox.Bottom, word.BoundingBox.Top - height));
                _heights.Add(height);
                _words.Add(word);
            }
            public bool IsSameLine(Word word, double height, string area) => _lastBottom < word.BoundingBox.Top && _lastTop > Math.Min(word.BoundingBox.Bottom, word.BoundingBox.Top - height) && Area == area;
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
                foreach (var word in _words.OrderBy(w => w.BoundingBox.Left).ThenByDescending(w => w.BoundingBox.Top).ToList())
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
        }
        private class ProcessingLines
        {
            private readonly List<ProcessingLine> _processingLines = new List<ProcessingLine>();
            private readonly Areas _areas;
            public ProcessingLines(Areas areas) => _areas = areas;
            public void AddWord(Word word, double height, Page page)
            {
                if (string.IsNullOrWhiteSpace(word.Text)) return;
                var area = _areas.GetAreaCode(word.BoundingBox, page);
                var processingLine = _processingLines.FirstOrDefault(i => i.IsSameLine(word, height, area));
                if (processingLine == null)
                {
                    processingLine = new ProcessingLine(word, height, area);
                    _processingLines.Add(processingLine);
                }
                else
                {
                    processingLine.AddWord(word, height);
                }
            }
            public List<(TextBlock, string)> GetTextLines() => _processingLines.Select(i =>
            {
                var lines = i.GetTextLines().ToList();
                return (new TextBlock(lines, "\t"), i.Area);
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
            return processingLines.GetTextLines().OrderByDescending(i => i.Item1.BoundingBox.Top).Select(i => new ProcessedBlock { KeepTogether = true, TextBlock = i.Item1, AreaCode = i.Item2 }).ToList();
        }
    }
    public class RecursiveXYSegmentMethod : ExtractMethod
    {
        public RecursiveXYSegmentMethod(WordExtractionType wordExtractionType, Areas areas) : base(wordExtractionType) { }

        protected override IEnumerable<TextBlock> GetTextGroups(Page page, IEnumerable<Word> words)
            => RecursiveXYCut.Instance.GetBlocks(words, new RecursiveXYCut.RecursiveXYCutOptions { MinimumWidth = page.Width / 3 });
    }
    public class DocstrumSegmentMethod : ExtractMethod
    {
        public DocstrumSegmentMethod(WordExtractionType wordExtractionType, Areas areas) : base(wordExtractionType) { }

        protected override IEnumerable<TextBlock> GetTextGroups(Page page, IEnumerable<Word> words)
            => DocstrumBoundingBoxes.Instance.GetBlocks(words,
                new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions()
                {
                    // WithinLineBounds = new DocstrumBoundingBoxes.AngleBounds(-45, 45),
                    // BetweenLineBounds = new DocstrumBoundingBoxes.AngleBounds(35, 170),
                    BetweenLineMultiplier = 1.5
                });
    }
    public class PdfZone
    {
        public double Left { get; set; }
        public double Width { get; set; }
        public double Top { get; set; }
        public double Height { get; set; }
        public int? PageNumber { get; set; }
        public bool IsInZone(UglyToad.PdfPig.Core.PdfRectangle rectangle) => rectangle.Bottom > this.Top
            && rectangle.Top < (this.Top + this.Height)
            && rectangle.Left < (this.Left + this.Width)
            && rectangle.Right > this.Left;
    }
    public class PdfReader : IDisposable
    {
        private readonly ExtractMethod _extractMethod;
        private readonly PdfDocument _pdfDocument;
        private IList<TextTemplate> _patternsToIgnore;
        private readonly StructureReader _structureReader;
        private readonly Areas _areas;

        public PdfReader(Stream pdfStream, IList<TextTemplate> patternsToIgnore = null, IList<HeadersSetup> titleSetups = null, ExtractMethod extractMethod = null, Areas areas = null)
        {

            _extractMethod = extractMethod ?? ExtractMethod.RecursiveXY();
            _extractMethod.Areas = areas ?? new Areas();
            _patternsToIgnore = patternsToIgnore;
            _pdfDocument = PdfDocument.Open(pdfStream);
            _structureReader = new StructureReader(_pdfDocument, titleSetups);
            _areas = areas ?? new Areas();
        }
        public void Dispose()
        {
            _pdfDocument.Dispose();
        }

        private bool IgnoreLine(TextLine textLine, Page page, List<HorizontalLine> lines) => _patternsToIgnore == null ? false : _patternsToIgnore.Any(i => i.Check(textLine, page, lines));
        private Grid TryAddInGrid(TextLine textLine, List<Grid> grids) // ALERT! this way to do prevents to have several grids at the same horizontal level (not likely to happen, but better to manage it)
        {
            Grid targetedGrid = null;
            foreach (var word in textLine.Words)
                foreach (var grid in grids)
                    if (grid.TryAddWord(word))
                        targetedGrid = grid;
            return targetedGrid;

        }
        // private IPageSegmenter CreateSegmenter() => _segmentMethod
        public void Read(IPdfVisitor pdfProcessor)
        {
            int lineNumberInPage = 0;
            int lineNumberInParagraph = 0;
            int lineNumber = 0;

            foreach (Page page in _pdfDocument.GetPages())
            {
                var gridExtractionResult = new GridExtractor().Extract(page);
                lineNumberInPage = 0;
                var words = page.GetWords(NearestNeighbourWordExtractor.Instance);
                Dictionary<Grid, List<string>> gridSections = new Dictionary<Grid, List<string>>();

                foreach (var block in _extractMethod.ExtractTextBlocks(page))
                {
                    if (block.KeepTogether)
                    {
                        bool inGrid = false;
                        foreach (var textLine in block.TextBlock.TextLines.Where(i => !IgnoreLine(i, page, gridExtractionResult.OutOfScopeHorizontalLines)))
                        {
                            var grid = this.TryAddInGrid(textLine, gridExtractionResult.Grids);
                            if (grid != null)
                            {
                                inGrid = true;
                                gridSections[grid] = _structureReader.Current;
                            }
                        }
                        if (!inGrid)
                        {
                            if (_structureReader.ProcessLine(new TextLine(block.TextBlock.TextLines.SelectMany(i => i.Words).ToList()), page, gridExtractionResult.OutOfScopeHorizontalLines))
                            {
                                lineNumberInParagraph = 0;
                                pdfProcessor.ProcessHeader(_structureReader.Current, page.Number);
                            }
                            else
                            {
                                pdfProcessor.ProcessLine(string.Join('\t', block.TextBlock.TextLines.Select(i => i.Text)), page.Number, ++lineNumber, ++lineNumberInParagraph, ++lineNumberInPage, _structureReader.Current, block.AreaCode);
                            }
                        }
                    }
                    else
                        foreach (var textLine in block.TextBlock.TextLines.Where(i => !IgnoreLine(i, page, gridExtractionResult.OutOfScopeHorizontalLines)))
                        {
                            var grid = this.TryAddInGrid(textLine, gridExtractionResult.Grids);
                            if (grid != null)
                            {
                                gridSections[grid] = _structureReader.Current;
                            }
                            else if (_structureReader.ProcessLine(textLine, page, gridExtractionResult.OutOfScopeHorizontalLines))
                            {
                                lineNumberInParagraph = 0;
                                pdfProcessor.ProcessHeader(_structureReader.Current, page.Number);
                            }
                            else
                            {
                                pdfProcessor.ProcessLine(textLine.Text, page.Number, ++lineNumber, ++lineNumberInParagraph, ++lineNumberInPage, _structureReader.Current, block.AreaCode);
                            }
                        }
                }
                foreach (var grid in gridExtractionResult.Grids.OrderByDescending(i => i.Top))
                {
                    var rows = grid.GetContent();
                    gridSections.TryGetValue(grid, out var section);
                    pdfProcessor.ProcessTable(rows, page.Number, section);
                }
            }
        }
    }
}