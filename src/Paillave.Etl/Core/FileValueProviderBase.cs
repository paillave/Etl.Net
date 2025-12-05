using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

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

    public async IAsyncEnumerable<FileReference> EnumerateAsync(object? input = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var fileReferences = new BlockingCollection<FileReference>();
        var producerTask = Task.Run(() =>
        {
            try
            {
                Provide(input, (fileValue, fileReference) => fileReferences.Add(fileReference, linkedCts.Token), linkedCts.Token);
            }
            catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
            {
            }
            finally
            {
                fileReferences.CompleteAdding();
            }
        }, CancellationToken.None);

        try
        {
            foreach (var fileReference in fileReferences.GetConsumingEnumerable(linkedCts.Token))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return fileReference;
                await Task.Yield();
            }
        }
        finally
        {
            linkedCts.Cancel();
            fileReferences.CompleteAdding();
            try
            {
                await producerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (linkedCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
            }
        }
    }

    public async IAsyncEnumerable<IFileValue> ProvideAsync(object? input = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var fileValues = new BlockingCollection<IFileValue>();
        var producerTask = Task.Run(() =>
        {
            try
            {
                Provide(input, (fileValue, fileReference) => fileValues.Add(fileValue, linkedCts.Token), linkedCts.Token);
            }
            catch (OperationCanceledException) when (linkedCts.IsCancellationRequested)
            {
            }
            finally
            {
                fileValues.CompleteAdding();
            }
        }, CancellationToken.None);

        try
        {
            foreach (var fileValue in fileValues.GetConsumingEnumerable(linkedCts.Token))
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return fileValue;
                await Task.Yield();
            }
        }
        finally
        {
            linkedCts.Cancel();
            fileValues.CompleteAdding();
            try
            {
                await producerTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (linkedCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
            }
        }
    }
    public Stream Open(JsonNode fileSpecific)
        => Provide(fileSpecific).GetContent();
}
