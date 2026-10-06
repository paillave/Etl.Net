using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        => Process(fileValue, _connectionParameters, _processorParameters, push, false, cancellationToken);
    public void Process(IFileValue fileValue, Action<IFileValue> push, bool useNewVersion, CancellationToken cancellationToken)
        => Process(fileValue, _connectionParameters, _processorParameters, push, useNewVersion, cancellationToken);
    /// <summary>
    /// Processors that don't need to support useNewVersion only override this method.
    /// </summary>
    protected virtual void Process(IFileValue fileValue, TConnectionParameters connectionParameters, TProcessorParameters processorParameters, Action<IFileValue> push, CancellationToken cancellationToken)
        => throw new NotImplementedException($"{Code}: Process is not implemented");
    /// <summary>
    /// Processors that can give back the file value of what they just saved override this method.
    /// By default, when useNewVersion is true, the source is read once into memory and the processor works on this copy,
    /// so that downstream reads never hit the source again (some sources delete the file once it has been read).
    /// </summary>
    protected virtual void Process(IFileValue fileValue, TConnectionParameters connectionParameters, TProcessorParameters processorParameters, Action<IFileValue> push, bool useNewVersion, CancellationToken cancellationToken)
    {
        if (!useNewVersion)
        {
            Process(fileValue, connectionParameters, processorParameters, push, cancellationToken);
            return;
        }
        byte[] content;
        using (var stream = fileValue.Get(false))
        using (var ms = new MemoryStream())
        {
            stream.CopyTo(ms);
            content = ms.ToArray();
        }
        var copy = new BufferedFileValue(content, fileValue.Name)
        {
            Metadata = fileValue.Metadata,
            Destinations = fileValue.Destinations
        };
        Process(copy, connectionParameters, processorParameters, push, cancellationToken);
    }
    private class BufferedFileValue(byte[] content, string name) : FileValueBase
    {
        public override string Name => name;
        public override Stream GetContent() => new MemoryStream(content, false);
        public override StreamWithResource OpenContent() => new(GetContent());
        protected override void DeleteFile() { }
    }
    /// <summary>
    /// Pushes either the input file value, or the new version built from what has been saved on the connector.
    /// Metadata and destinations are carried over to the new version.
    /// </summary>
    protected static void PushResult(IFileValue fileValue, bool useNewVersion, Func<IFileValue> createNewVersion, Action<IFileValue> push)
    {
        if (!useNewVersion)
        {
            push(fileValue);
            return;
        }
        var newVersion = createNewVersion();
        newVersion.Metadata = fileValue.Metadata;
        newVersion.Destinations = fileValue.Destinations;
        push(newVersion);
    }
    public void Test() => Test(_connectionParameters, _processorParameters);
    protected abstract void Test(TConnectionParameters connectionParameters, TProcessorParameters processorParameters);

    public IAsyncEnumerable<IFileValue> ProcessAsync(IFileValue input, CancellationToken cancellationToken = default)
        => ProcessAsync(input, false, cancellationToken);
    public async IAsyncEnumerable<IFileValue> ProcessAsync(IFileValue input, bool useNewVersion, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var fileValues = new BlockingCollection<IFileValue>();
        var producerTask = Task.Run(() =>
        {
            try
            {
                Process(input, fileValue => fileValues.Add(fileValue, linkedCts.Token), useNewVersion, linkedCts.Token);
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
