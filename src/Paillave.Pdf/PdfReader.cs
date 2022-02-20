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
    }

    public abstract class ExtractMethod
    {
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
        public static ExtractMethod RecursiveXY(WordExtractionType wordExtractionType = WordExtractionType.NearestNeighbour) => new RecursiveXYSegmentMethod(wordExtractionType);
        public static ExtractMethod Docstrum(WordExtractionType wordExtractionType = WordExtractionType.NearestNeighbour) => new DocstrumSegmentMethod(wordExtractionType);
        public static ExtractMethod SimpleLines(WordExtractionType wordExtractionType = WordExtractionType.NearestNeighbour) => new SimpleLinesMethod(wordExtractionType);
        protected virtual IEnumerable<TextBlock> GetTextGroups(Page page, IEnumerable<Word> words)
        {
            yield break;
        }
        internal virtual List<ProcessedBlock> ExtractTextBlocks(Page page)
        {
            var words = page.GetWords(this.WordExtractor);
            var blocks = GetTextGroups(page, words)
                // .OrderByDescending(i => /*i.TextOrientation==Tex*/  i.BoundingBox.Top)
                .ToList();
            return blocks.Select(i => new ProcessedBlock { KeepTogether = false, TextBlock = i }).ToList();//.SelectMany(i => i.TextLines).ToList();
        }
        protected IWordExtractor WordExtractor { get; }
    }
    public class SimpleLinesMethod : ExtractMethod
    {
        public SimpleLinesMethod(WordExtractionType wordExtractionType) : base(wordExtractionType) { }
        private class ProcessingLine
        {
            private readonly List<Word> _words = new List<Word>();
            private readonly List<double> _heights = new List<double>();
            private double _lastTop;
            private double _lastBottom;
            public ProcessingLine(Word word)
            {
                (_lastTop, _lastBottom) = (word.BoundingBox.Top, word.BoundingBox.Bottom);
                _heights.Add(word.BoundingBox.Height);
                _words.Add(word);
            }
            public bool IsSameLine(Word word) => _lastBottom < word.BoundingBox.Top && _lastTop > word.BoundingBox.Bottom;
            public void AddWord(Word word)
            {
                _lastTop = word.BoundingBox.Top;
                _lastBottom = word.BoundingBox.Bottom;
                _heights.Add(word.BoundingBox.Height);
                _words.Add(word);
            }
            public IEnumerable<TextLine> GetTextLines()
            {
                var maxSpace = _heights.Average();
                Word previousWord = null;
                List<Word> currentBlock = new List<Word>();
                foreach (var word in _words.OrderBy(w => w.BoundingBox.Left).ToList())
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
            public void AddWord(Word word)
            {
                if (string.IsNullOrWhiteSpace(word.Text)) return;
                var processingLine = _processingLines.FirstOrDefault(i => i.IsSameLine(word));
                if (processingLine == null)
                {
                    processingLine = new ProcessingLine(word);
                    _processingLines.Add(processingLine);
                }
                else
                {
                    processingLine.AddWord(word);
                }
            }
            public List<TextBlock> GetTextLines() => _processingLines.Select(i =>
            {
                var lines = i.GetTextLines().ToList();
                return new TextBlock(lines, "\t");
            }).ToList();
        }
        internal override List<ProcessedBlock> ExtractTextBlocks(Page page)
        {
            var processingLines = new ProcessingLines();
            var words = page.GetWords(this.WordExtractor).OrderByDescending(i => i.BoundingBox.Top).ThenBy(i => i.BoundingBox.Left).ToList();
            foreach (var word in words)
                processingLines.AddWord(word);
            return processingLines.GetTextLines().Select(i => new ProcessedBlock { KeepTogether = true, TextBlock = i }).ToList();
        }
    }
    public class RecursiveXYSegmentMethod : ExtractMethod
    {
        public RecursiveXYSegmentMethod(WordExtractionType wordExtractionType) : base(wordExtractionType) { }

        protected override IEnumerable<TextBlock> GetTextGroups(Page page, IEnumerable<Word> words)
            => RecursiveXYCut.Instance.GetBlocks(words, new RecursiveXYCut.RecursiveXYCutOptions { MinimumWidth = page.Width / 3 });
    }
    public class DocstrumSegmentMethod : ExtractMethod
    {
        public DocstrumSegmentMethod(WordExtractionType wordExtractionType) : base(wordExtractionType) { }

        protected override IEnumerable<TextBlock> GetTextGroups(Page page, IEnumerable<Word> words)
            => DocstrumBoundingBoxes.Instance.GetBlocks(words,
                new DocstrumBoundingBoxes.DocstrumBoundingBoxesOptions()
                {
                    // WithinLineBounds = new DocstrumBoundingBoxes.AngleBounds(-45, 45),
                    // BetweenLineBounds = new DocstrumBoundingBoxes.AngleBounds(35, 170),
                    BetweenLineMultiplier = 1.5
                });
    }
    public class PdfReader : IDisposable
    {
        private readonly ExtractMethod _extractMethod;
        private readonly PdfDocument _pdfDocument;
        private IList<TextTemplate> _patternsToIgnore;
        private readonly StructureReader _structureReader;

        public PdfReader(Stream pdfStream, IList<TextTemplate> patternsToIgnore = null, IList<HeadersSetup> titleSetups = null, ExtractMethod extractMethod = null)
        {
            _extractMethod = extractMethod ?? ExtractMethod.RecursiveXY();
            _patternsToIgnore = patternsToIgnore;
            _pdfDocument = PdfDocument.Open(pdfStream);
            _structureReader = new StructureReader(_pdfDocument, titleSetups);
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
                                pdfProcessor.ProcessLine(string.Join('\t', block.TextBlock.TextLines.Select(i => i.Text)), page.Number, ++lineNumber, ++lineNumberInParagraph, ++lineNumberInPage, _structureReader.Current);
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
                                pdfProcessor.ProcessLine(textLine.Text, page.Number, ++lineNumber, ++lineNumberInParagraph, ++lineNumberInPage, _structureReader.Current);
                            }
                        }
                }


                // foreach (var textLine in _extractMethod.ExtractTextBlocks(page).SelectMany(i => i.TextLines).Where(i => !IgnoreLine(i, page, gridExtractionResult.OutOfScopeHorizontalLines)))
                // {
                //     var grid = this.TryAddInGrid(textLine, gridExtractionResult.Grids);
                //     if (grid != null)
                //     {
                //         gridSections[grid] = _structureReader.Current;
                //     }
                //     else if (_structureReader.ProcessLine(textLine, page, gridExtractionResult.OutOfScopeHorizontalLines))
                //     {
                //         lineNumberInParagraph = 0;
                //         pdfProcessor.ProcessHeader(_structureReader.Current, page.Number);
                //     }
                //     else
                //     {
                //         pdfProcessor.ProcessLine(textLine.Text, page.Number, ++lineNumber, ++lineNumberInParagraph, ++lineNumberInPage, _structureReader.Current);
                //     }
                // }
                // var blocks = RecursiveXYCut.Instance
                //     .GetBlocks(words, new RecursiveXYCut.RecursiveXYCutOptions { MinimumWidth = page.Width / 3 })
                //     .OrderByDescending(i => /*i.TextOrientation==Tex*/  i.BoundingBox.Top)
                //     .ToList();
                // Dictionary<Grid, List<string>> gridSections = new Dictionary<Grid, List<string>>();
                // foreach (var block in blocks)
                // {
                //     foreach (var textLine in block.TextLines.Where(i => !IgnoreLine(i, page, gridExtractionResult.OutOfScopeHorizontalLines)))
                //     {
                //         var grid = this.TryAddInGrid(textLine, gridExtractionResult.Grids);
                //         if (grid != null)
                //         {
                //             gridSections[grid] = _structureReader.Current;
                //         }
                //         else if (_structureReader.ProcessLine(textLine, page, gridExtractionResult.OutOfScopeHorizontalLines))
                //         {
                //             lineNumberInParagraph = 0;
                //             pdfProcessor.ProcessHeader(_structureReader.Current, page.Number);
                //         }
                //         else
                //         {
                //             pdfProcessor.ProcessLine(textLine.Text, page.Number, ++lineNumber, ++lineNumberInParagraph, ++lineNumberInPage, _structureReader.Current);
                //         }
                //     }
                // }
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