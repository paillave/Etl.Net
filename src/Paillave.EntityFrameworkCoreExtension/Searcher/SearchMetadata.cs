using System.Collections.Generic;

namespace Paillave.EntityFrameworkCoreExtension.Searcher;

public class SearchMetadata
{
    // public SearchMetadata()
    // {

    // }
    // public SearchMetadata()
    // {
    //     Type = type;
    //     Name = name;
    //     SubLevels = subLevels;
    // }

    public required string Name { get; set; }
    public required string Type { get; set; }
    public List<SearchMetadata>? SubLevels { get; set; }=null;
}
