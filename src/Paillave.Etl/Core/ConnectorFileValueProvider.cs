using System;
using System.Threading;

namespace Paillave.Etl.Core;

public class ConnectorFileValueProvider<TIn>(IFileValueProvider fileValueProvider) : IValuesProvider<TIn, IFileValue>
{
    public string TypeName => $"ConnectorFilesSource_{fileValueProvider.Code}";
    public ProcessImpact PerformanceImpact => fileValueProvider.PerformanceImpact;
    public ProcessImpact MemoryFootPrint => fileValueProvider.MemoryFootPrint;
    public void PushValues(TIn input, Action<IFileValue, FileReference> push, CancellationToken cancellationToken) => fileValueProvider.Provide(input, push, cancellationToken);
    public void PushValues(TIn input, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context) => this.PushValues(input, (fv, fr) => push(fv), cancellationToken);
}