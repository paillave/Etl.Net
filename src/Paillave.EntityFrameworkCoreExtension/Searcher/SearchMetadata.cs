using System.Collections.Generic;

namespace Paillave.EntityFrameworkCoreExtension.Searcher
{
    public class SearchMetadata
    {
        public SearchMetadata()
        {

        }
        public SearchMetadata(string name, string type, List<SearchMetadata> subLevels = null)
        {
            Type = type;
            Name = name;
            SubLevels = subLevels;
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public List<SearchMetadata> SubLevels { get; set; }
    }
}