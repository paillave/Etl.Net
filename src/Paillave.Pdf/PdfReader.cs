using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;

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
        public HashSet<string> AreaCodes { get; set; }
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
                                pdfProcessor.ProcessLine(string.Join('\t', block.TextBlock.TextLines.Select(i => i.Text)), page.Number, ++lineNumber, ++lineNumberInParagraph, ++lineNumberInPage, _structureReader.Current, block.AreaCodes);
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
                                pdfProcessor.ProcessLine(textLine.Text, page.Number, ++lineNumber, ++lineNumberInParagraph, ++lineNumberInPage, _structureReader.Current, block.AreaCodes);
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