using System.Collections.Generic;
using System.Linq;
using UglyToad.PdfPig.Content;

namespace Paillave.Pdf
{
    public class Areas : Dictionary<string, PdfZone>
    {
        public HashSet<string> GetAreaCodes(UglyToad.PdfPig.Core.PdfRectangle rectangle, Page page)
                => this.Where(a =>
                {
                    if (a.Value.PageNumber != null && page.Number != a.Value.PageNumber.Value)
                        return false;
                    return a.Value.IsInZone(rectangle);
                }).Select(i => i.Key).ToHashSet();
    }
}