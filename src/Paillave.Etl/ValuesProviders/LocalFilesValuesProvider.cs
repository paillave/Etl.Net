using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.StreamNodes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Paillave.Etl.ValuesProviders
{
    public class LocalFilesValue
    {
        public LocalFilesValue(string name)
        {
            this.Name = name;
        }
        public string Name { get; }

        public Stream GetContent()
        {
            return File.OpenRead(this.Name);
        }
    }
    public class LocalFilesValuesProviderArgs
    {
        public string SearchPattern { get; set; } = "*";
        public bool Recursive { get; set; } = false;
        public string RootFolder { get; set; }
    }
    public class LocalFilesValuesProvider
    {
        public void PushValues(Action<LocalFilesValue> pushValue, LocalFilesValuesProviderArgs args)
        {
            Directory
                .GetFiles(args.RootFolder, args.SearchPattern, args.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .ToList()
                .ForEach(name => pushValue(new LocalFilesValue(name)));
        }
    }
}
