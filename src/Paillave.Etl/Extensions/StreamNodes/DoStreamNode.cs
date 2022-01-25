using System;
using System.Threading;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    #region Actions Without Resources
    public interface IDoProcessor<TIn>
    {
        void ProcessRow(TIn value, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker);
    }
    public class SimpleDoProcessor<TIn, TInnerIn> : IDoProcessor<TIn>
    {
        private readonly Func<TIn, TInnerIn> _getInner;
        private readonly Action<TInnerIn> _processRow;
        public SimpleDoProcessor(Func<TIn, TInnerIn> getInner, Action<TInnerIn> processRow)
            => (_processRow, _getInner) = (processRow, getInner);
        public void ProcessRow(TIn value, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            _processRow(_getInner(value));
        }
    }
    public class DoArgs<TIn, TStream> where TStream : IStream<TIn>
    {
        public TStream Stream { get; set; }
        public IDoProcessor<TIn> Processor { get; set; }
    }
    public class DoStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, DoArgs<TIn, TStream>> where TStream : IStream<TIn>
    {
        public DoStreamNode(string name, DoArgs<TIn, TStream> args) : base(name, args) { }
        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        protected override TStream CreateOutputStream(DoArgs<TIn, TStream> args)
        {
            return base.CreateMatchingStream(args.Stream.Observable.Do(i => args.Processor.ProcessRow(i, CancellationToken.None, args.Stream.SourceNode.ExecutionContext.DependencyResolver, args.Stream.SourceNode.ExecutionContext)), args.Stream);
        }
    }
    #endregion
    public class DoWithResolutionProcessor<TIn, TInnerIn, TService> : IDoProcessor<TIn> where TService : class
    {
        private readonly Action<TInnerIn, TService, CancellationToken, IInvoker> _actionFull = null;
        private readonly Action<TInnerIn, TService> _actionSimple = null;
        private readonly Func<TIn, TInnerIn> _getInner;
        private TService _service = null;
        private readonly object _lock = new Object();
        public DoWithResolutionProcessor(Action<TInnerIn, TService, CancellationToken, IInvoker> actionFull, Func<TIn, TInnerIn> getInner) => (_actionFull, _getInner) = (actionFull, getInner);
        public DoWithResolutionProcessor(Action<TInnerIn, TService> actionSimple, Func<TIn, TInnerIn> getInner) => (_actionSimple, _getInner) = (actionSimple, getInner);

        public void ProcessRow(TIn value, CancellationToken cancellationToken, IDependencyResolver resolver, IInvoker invoker)
        {
            lock (_lock)
            {
                if (_service == null)
                {
                    _service = resolver.Resolve<TService>();
                }
            }
            if (_actionFull != null)
            {
                _actionFull(_getInner(value), _service, cancellationToken, invoker);
            }
            else if (_actionSimple != null)
            {
                _actionSimple(_getInner(value), _service);
            }
        }
    }
    public class DoWithResolutionProcessorBuilder<TIn, TInnerIn>
    {
        private readonly Func<TIn, TInnerIn> _getInner;
        public DoWithResolutionProcessorBuilder(Func<TIn, TInnerIn> getInner) => _getInner = getInner;
        public DoWithResolutionProcessorBuilder<TIn, TInnerIn, TService> Resolve<TService>() where TService : class => new DoWithResolutionProcessorBuilder<TIn, TInnerIn, TService>(_getInner);
    }
    public class DoWithResolutionProcessorBuilder<TIn, TInnerIn, TService> where TService : class
    {
        private readonly Func<TIn, TInnerIn> _getInner;
        public DoWithResolutionProcessorBuilder(Func<TIn, TInnerIn> getInner) => _getInner = getInner;
        public IDoProcessor<TIn> ThenDo(Action<TInnerIn, TService, CancellationToken, IInvoker> actionFull) => new DoWithResolutionProcessor<TIn, TInnerIn, TService>(actionFull, _getInner);
        public IDoProcessor<TIn> ThenDo(Action<TInnerIn, TService> actionSimple) => new DoWithResolutionProcessor<TIn, TInnerIn, TService>(actionSimple, _getInner);
    }
}
