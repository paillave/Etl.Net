using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

namespace Paillave.Pdf
{
    public class PdfReader : IDisposable
    {
        private readonly PdfDocument _pdfDocument;
        private IList<TextTemplate> _patternsToIgnore;
        private readonly StructureReader _structureReader;

        public PdfReader(Stream pdfStream, IList<TextTemplate> patternsToIgnore = null, IList<HeadersSetup> titleSetups = null)
        {
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
        public void Read(IPdfProcessor pdfProcessor)
        {
            int lineNumberInPage = 0;
            int lineNumberInParagraph = 0;
            int lineNumber = 0;

            foreach (Page page in _pdfDocument.GetPages())
            {
                lineNumberInPage = 0;
                var words = page.GetWords(NearestNeighbourWordExtractor.Instance);
                var gridExtractionResult = new GridExtractor().Extract(page);
                var blocks = RecursiveXYCut.Instance
                    .GetBlocks(words, new RecursiveXYCut.RecursiveXYCutOptions { MinimumWidth = page.Width / 3 })
                    .OrderByDescending(i => i.BoundingBox.Top)
                    .ToList();
                Dictionary<Grid, List<string>> gridSections = new Dictionary<Grid, List<string>>();
                foreach (var block in blocks)
                {
                    foreach (var textLine in block.TextLines.Where(i => !IgnoreLine(i, page, gridExtractionResult.OutOfScopeHorizontalLines)))
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