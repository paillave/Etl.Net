using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Connector;
using Paillave.Etl.Reactive.Operators;
using Paillave.Etl.ValuesProviders;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.StreamNodes
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
            _processor = this.ExecutionContext.Connectors.GetProcessor(args.OutputConnectorCode);
            PerformanceImpact = _processor.PerformanceImpact;
            MemoryFootPrint = _processor.MemoryFootPrint;
        }
        public override string TypeName => $"ConnectorFileTarget{_processor.Code}";
        public override ProcessImpact PerformanceImpact { get; }
        public override ProcessImpact MemoryFootPrint { get; }
        protected override IStream<IFileValue> CreateOutputStream(ProcessFileToConnectorArgs args)
            => base.CreateUnsortedStream(args.Input.Observable.FlatMap((i, ct) => new DeferredPushObservable<IFileValue>((af, c) =>
             {
                 _processor.Process(i, af, c, this.ExecutionContext.DependencyResolver);
             }, ct)));
    }
}