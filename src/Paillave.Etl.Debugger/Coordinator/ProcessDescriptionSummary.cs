using System;
using System.Collections.Generic;
using System.Reflection;

namespace Paillave.Etl.Debugger.Coordinator
{
    public class ProcessDescriptionSummary
    {
        public string AssemblyFilePath { get; set; }
        public string StreamTransformationName { get; set; }
        public string ClassName { get; set; }
        public string Namespace { get; set; }
        public List<string> Parameters { get; set; }
    }
}