using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Paillave.Etl.Core.ContentProcessors
{
    public interface IContentItem
    {
        string Name { get; }
        Stream GetContent();
    }
}
