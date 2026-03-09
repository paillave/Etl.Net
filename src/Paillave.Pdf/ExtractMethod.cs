using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis;
using UglyToad.PdfPig.DocumentLayoutAnalysis.WordExtractor;
using UglyToad.PdfPig.Util;

namespace Paillave.Pdf;

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
                Areas = this.Areas.GetAreaCodes(word.BoundingBox, page),
                Word = word
            })
            .GroupBy(i => i.Areas.FirstOrDefault() ?? "")
            .SelectMany(i => GetTextGroups(page, i.Select(i => i.Word).ToList()).Select(j => new ProcessedBlock { KeepTogether = false, AreaCodes = new HashSet<string> { i.Key }, TextBlock = j }))
            .OrderByDescending(i => i.TextBlock.BoundingBox.Top)
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