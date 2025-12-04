using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading;

namespace Paillave.Etl.Core;

public abstract class FileValueProviderBase<TConnectionParameters, TProviderParameters>(string code, string name, string connectionName, TConnectionParameters connectionParameters, TProviderParameters providerParameters) : IFileValueProvider, IValuesProvider<object, IFileValue> // where TConnectionParameters : class, new() where TProviderParameters : class, new()
{
    public string Code { get; } = code;
    public abstract ProcessImpact PerformanceImpact { get; }
    public abstract ProcessImpact MemoryFootPrint { get; }
    protected string ConnectionName { get; } = connectionName;
    protected string Name { get; } = name;

    public virtual string TypeName => this.GetType().Name;

    public void Provide(object? input, Action<IFileValue, FileReference> pushFileValue, CancellationToken cancellationToken)
        => Provide(input, pushFileValue, connectionParameters, providerParameters, cancellationToken);
    public void Provide(object? input, Action<IFileValue> pushFileValue, CancellationToken cancellationToken)
        => Provide(input, (fileValue, FileReference) => pushFileValue(fileValue), cancellationToken);
    protected abstract void Provide(object? input, Action<IFileValue, FileReference> pushFileValue, TConnectionParameters connectionParameters, TProviderParameters providerParameters, CancellationToken cancellationToken);
    public void Test() => Test(connectionParameters, providerParameters);
    protected abstract void Test(TConnectionParameters connectionParameters, TProviderParameters providerParameters);
    public void PushValues(object input, Action<IFileValue> push, CancellationToken cancellationToken, IExecutionContext context)
        => Provide(input, push, cancellationToken);

    public abstract IFileValue Provide(JsonNode? fileSpecific);

    public IAsyncEnumerable<FileReference> Provide(object? input = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Stream Open(JsonNode fileSpecific)
    {
        throw new NotImplementedException();
    }
}