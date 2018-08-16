using System.Collections.Generic;

namespace Paillave.Etl.ContentProcessors
{
    public interface IContentProvider
    {
        IEnumerable<IContentItem> GetContentItems(string pattern, bool recursive);
    }
}
