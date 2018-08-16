using Paillave.Etl.Core;
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
    }

    public class LocalFilesValuesProvider : IValuesProvider<LocalFilesValuesProviderArgs, LocalFilesValue>
    {
        private readonly string _rootFolder;

        public LocalFilesValuesProvider(string rootFolder)
        {
            this._rootFolder = rootFolder;
        }

        public void Dispose() { }

        public void PushValues(LocalFilesValuesProviderArgs args, Action<LocalFilesValue> pushValue)
        {
            Directory
                .GetFiles(this._rootFolder, args.SearchPattern, args.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .ToList()
                .ForEach(name => new LocalFilesValue(name));
        }
    }
}
