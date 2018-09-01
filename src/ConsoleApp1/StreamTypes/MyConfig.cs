using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsoleApp1.StreamTypes
{
    public class MyConfig
    {
        public string InputFolderPath { get; set; }
        public string InputFilesSearchPattern { get; set; }
        public string TypeFilePath { get; set; }
        public string DestinationFilePath { get; internal set; }
        public string CategoryDestinationFilePath { get; internal set; }
    }
}
