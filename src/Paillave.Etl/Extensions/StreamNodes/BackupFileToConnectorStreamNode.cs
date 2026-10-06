using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Reactive.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Paillave.Etl.Core;

public class BackupFileToConnectorArgs
{
    public IStream<IFileValue> Input { get; set; }
    public string BackupConnectorCode { get; set; }
    public bool DeleteFromSource { get; set; }
}
public class BackupFileToConnectorStreamNode : StreamNodeBase<IFileValue, IStream<IFileValue>, BackupFileToConnectorArgs>
{
    private readonly IFileValueProcessor _processor;
    public BackupFileToConnectorStreamNode(string name, BackupFileToConnectorArgs args) : base(name, args)
    {
        _processor = this.ExecutionContext.Services.GetRequiredService<IFileValueConnectors>().GetProcessor(args.BackupConnectorCode);
        PerformanceImpact = _processor.PerformanceImpact;
        MemoryFootPrint = _processor.MemoryFootPrint;
    }
    public override string TypeName => $"ConnectorFileBackup{_processor.Code}";
    public override ProcessImpact PerformanceImpact { get; }
    public override ProcessImpact MemoryFootPrint { get; }
    protected override IStream<IFileValue> CreateOutputStream(BackupFileToConnectorArgs args)
        => base.CreateUnsortedStream(args.Input.Observable.FlatMap((i, ct) => new DeferredPushObservable<IFileValue>((push, c) =>
        {
            var backedUpFileValues = new List<IFileValue>();
            _processor.Process(i, backedUpFileValues.Add, true, c);
            if (args.DeleteFromSource)
            {
                try
                {
                    i.Delete();
                }
                catch { }
            }
            foreach (var backedUpFileValue in backedUpFileValues)
                push(backedUpFileValue);
        }, ct)));
}
