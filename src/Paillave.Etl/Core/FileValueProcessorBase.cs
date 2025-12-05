using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.Etl.Core;

public abstract class FileValueProcessorBase<TConnectionParameters, TProcessorParameters>(string code, string name, string connectionName, TConnectionParameters connectionParameters, TProcessorParameters processorParameters) : IFileValueProcessor
{
    public string Code { get; } = code;
    public abstract ProcessImpact PerformanceImpact { get; }
    public abstract ProcessImpact MemoryFootPrint { get; }
    protected string ConnectionName { get; } = connectionName;
    protected string Name { get; } = name;
    private readonly TConnectionParameters _connectionParameters = connectionParameters;
    private readonly TProcessorParameters _processorParameters = processorParameters;

    public void Process(IFileValue fileValue, Action<IFileValue> push, CancellationToken cancellationToken)
        => Process(fileValue, _connectionParameters, _processorParameters, push, cancellationToken);
    protected abstract void Process(IFileValue fileValue, TConnectionParameters connectionParameters, TProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken);
    public void Test() => Test(_connectionParameters, _processorParameters);
    protected abstract void Test(TConnectionParameters connectionParameters, TProcessorParameters processorParameters);

    public async IAsyncEnumerable<IFileValue> ProcessAsync(IFileValue input, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var fileValues = new BlockingCollection<IFileValue>();
        var producerTask = Task.Run(() =>
        {
            try
            {
                Process(input, fileValue => fileValues.Add(fileValue, linkedCts.Token), linkedCts.Token);
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
}
