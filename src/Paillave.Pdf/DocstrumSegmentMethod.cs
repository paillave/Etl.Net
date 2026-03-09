using System.Collections.Generic;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using static UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter.DocstrumBoundingBoxes;

namespace Paillave.Pdf;

public class DocstrumSegmentMethod(WordExtractionType wordExtractionType, Areas areas) : ExtractMethod(wordExtractionType)
{
    protected override IEnumerable<TextBlock> GetTextGroups(Page page, IEnumerable<Word> words)
        => new DocstrumBoundingBoxes(new DocstrumBoundingBoxesOptions()
        {
            // WithinLineBounds = new DocstrumBoundingBoxes.AngleBounds(-45, 45),
            // BetweenLineBounds = new DocstrumBoundingBoxes.AngleBounds(35, 170),
            BetweenLineMultiplier = 1.5
        }).GetBlocks(words);
}