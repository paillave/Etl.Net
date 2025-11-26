using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.Reactive.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Paillave.Etl.Core
{
    public class ProcessFileToConnectorArgs
    {
        public IStream<IFileValue> Input { get; set; }
        public string OutputConnectorCode { get; set; }
    }
    public class ProcessFileToConnectorStreamNode : StreamNodeBase<IFileValue, IStream<IFileValue>, ProcessFileToConnectorArgs>
    {
        private IFileValueProcessor _processor;
        public ProcessFileToConnectorStreamNode(string name, ProcessFileToConnectorArgs args) : base(name, args)
        {
            _processor = this.ExecutionContext.Services.GetRequiredService<IFileValueConnectors>().GetProcessor(args.OutputConnectorCode);
            PerformanceImpact = _processor.PerformanceImpact;
            MemoryFootPrint = _processor.MemoryFootPrint;
        }
        public override string TypeName => $"ConnectorFileTarget{_processor.Code}";
        public override ProcessImpact PerformanceImpact { get; }
        public override ProcessImpact MemoryFootPrint { get; }
        protected override IStream<IFileValue> CreateOutputStream(ProcessFileToConnectorArgs args)
            => base.CreateUnsortedStream(args.Input.Observable.FlatMap((i, ct) => new DeferredPushObservable<IFileValue>((af, c) =>
             {
                 _processor.Process(i, af, c);
             }, ct)));
    }
}