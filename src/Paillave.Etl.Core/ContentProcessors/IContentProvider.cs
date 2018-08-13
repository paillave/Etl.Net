using System.Collections.Generic;

namespace Paillave.Etl.Core.ContentProcessors
{
    public interface IContentProvider
    {
        IEnumerable<IContentItem> GetContentItems(string pattern, bool recursive);
    }
}
