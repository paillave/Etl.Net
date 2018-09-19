using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SubProcessQuickStart.StreamTypes
{
    public class MyConfig
    {
        public string InputFolderPath { get; set; }
        public string InputFilesSearchPattern { get; set; }
        public string TypeFilePath { get; set; }
        public string DestinationFilePath { get; internal set; }
        public string CategoryDestinationFolder { get; internal set; }
    }
}
