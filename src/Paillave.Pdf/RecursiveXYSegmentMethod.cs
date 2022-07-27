using System.Collections.Generic;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
using static UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter.RecursiveXYCut;

namespace Paillave.Pdf
{
    public class RecursiveXYSegmentMethod : ExtractMethod
    {
        public RecursiveXYSegmentMethod(WordExtractionType wordExtractionType, Areas areas) : base(wordExtractionType) { }

        protected override IEnumerable<TextBlock> GetTextGroups(Page page, IEnumerable<Word> words)
            => new RecursiveXYCut(new RecursiveXYCutOptions { MinimumWidth = page.Width / 3 }).GetBlocks(words);
    }
}