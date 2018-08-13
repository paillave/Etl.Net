using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Paillave.Etl.Core.ContentProcessors
{
    public class PhysicalContentProvider : IContentProvider
    {
        private readonly string _rootFolder;

        public PhysicalContentProvider(string rootFolder)
        {
            this._rootFolder = rootFolder;
        }
        public IEnumerable<IContentItem> GetContentItems(string searchPattern, bool recursive)
        {
            return Directory
                .GetFiles(this._rootFolder, searchPattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(name => new PhysicalContentItem(name))
                .ToList();
        }
    }
}
