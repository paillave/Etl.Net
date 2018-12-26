using System;
using System.Reflection;

namespace Paillave.Etl.Debugger.Coordinator
{
    public class ProcessDescription
    {
        public ProcessDescriptionSummary Summary { get; internal set; }
        public Type StreamConfigType { get; internal set; }
        public Type StreamType { get; internal set; }
        public MethodInfo StreamTransformationMethodInfo { get; internal set; }
        public Type ClassType { get; internal set; }
    }
}