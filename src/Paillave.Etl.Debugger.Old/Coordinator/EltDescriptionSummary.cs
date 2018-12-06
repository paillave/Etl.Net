using System;
using System.Reflection;

namespace Paillave.Etl.Debugger.Coordinator
{
    public class EltDescriptionSummary
    {
        public string AssemblyFilePath { get; set; }
        public string StreamTransformationName { get; internal set; }
        public string ClassName { get; internal set; }
        public string Namespace { get; internal set; }
    }
}