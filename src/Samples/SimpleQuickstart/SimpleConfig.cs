using Paillave.Etl;
using System;
using System.IO;
using Paillave.Etl.Core;
using Paillave.Etl.TextFile.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.TextFile;

namespace SimpleQuickstart
{
    public class SimpleConfig
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
    }
}
