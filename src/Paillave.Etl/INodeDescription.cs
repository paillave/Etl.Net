using Paillave.Etl.Core;

namespace Paillave.Etl
{
    public interface INodeDescription
    {
        string NodeName { get; }
        string TypeName { get; }
        ProcessImpact PerformanceImpact { get; }
        ProcessImpact MemoryFootPrint { get; }
        bool IsRootNode { get; }
    }
}
