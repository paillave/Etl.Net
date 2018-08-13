using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Paillave.Etl.Core.ContentProcessors
{
    public class PhysicalContentItem : IContentItem
    {
        public PhysicalContentItem(string name)
        {
            this.Name = name;
        }
        public string Name { get ; }

        public Stream GetContent()
        {
            return File.OpenRead(this.Name);
        }
    }
}
