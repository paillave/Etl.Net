namespace Paillave.Etl.Core;

public interface INodeDescription
{
    string NodeName { get; }
    string TypeName { get; }
    ProcessImpact PerformanceImpact { get; }
    ProcessImpact MemoryFootPrint { get; }
    INodeDescription? Parent { get; }
}
