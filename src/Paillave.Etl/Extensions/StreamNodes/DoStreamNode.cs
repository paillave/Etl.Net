using System;
using Paillave.Etl.Reactive.Operators;

namespace Paillave.Etl.Core
{
    #region Actions Without Resources
    public interface IDoProcessor<TIn>
    {
        void ProcessRow(TIn value);
    }
    public class SimpleDoProcessor<TIn> : IDoProcessor<TIn>
    {
        private Action<TIn> _processRow;
        public SimpleDoProcessor(Action<TIn> processRow)
        {
            _processRow = processRow;
        }
        public void ProcessRow(TIn value)
        {
            _processRow(value);
        }
    }
    public class ContextDoProcessor<TIn, TCtx> : IDoProcessor<TIn>
    {
        private Action<TIn, TCtx, Action<TCtx>> _processRow;
        private TCtx _context;
        public ContextDoProcessor(Action<TIn, TCtx, Action<TCtx>> processRow, TCtx initialContext)
        {
            _processRow = processRow;
            _context = initialContext;
        }
        public void ProcessRow(TIn value)
        {
            _processRow(value, _context, SetContext);
        }
        private void SetContext(TCtx newContext)
        {
            _context = newContext;
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
            return base.CreateMatchingStream(args.Stream.Observable.Do(args.Processor.ProcessRow), args.Stream);
        }
    }
    #endregion

    #region Actions With Resources
    public interface IDoProcessor<TIn, TResource>
    {
        void PreProcess(TResource resource);
        void ProcessRow(TIn value, TResource resource);
    }
    public class SimpleDoProcessor<TIn, TResource> : IDoProcessor<TIn, TResource>
    {
        private Action<TIn, TResource> _processRow;
        private Action<TResource> _preProcess;
        public SimpleDoProcessor(Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
        {
            _preProcess = preProcess;
            _processRow = processRow;
        }
        public void PreProcess(TResource resource)
        {
            _preProcess?.Invoke(resource);
        }

        public void ProcessRow(TIn value, TResource resource)
        {
            _processRow(value, resource);
        }
    }
    public class ContextDoProcessor<TIn, TResource, TCtx> : IDoProcessor<TIn, TResource>
    {
        private Action<TIn, TResource, TCtx, Action<TCtx>> _processRow;
        private Action<TResource, Action<TCtx>> _preProcess;
        private TCtx _context;
        public ContextDoProcessor(Action<TIn, TResource, TCtx, Action<TCtx>> processRow, Action<TResource, Action<TCtx>> preProcess = null)
        {
            _preProcess = preProcess;
            _processRow = processRow;
            _context = default(TCtx);
        }
        public void ProcessRow(TIn value, TResource resource)
        {
            _processRow(value, resource, _context, SetContext);
        }
        public void PreProcess(TResource resource)
        {
            _preProcess?.Invoke(resource, SetContext);
        }
        private void SetContext(TCtx newContext)
        {
            _context = newContext;
        }
    }
    public class DoArgs<TIn, TStream, TResource> where TStream : IStream<TIn>
    {
        public TStream Stream { get; set; }
        public ISingleStream<TResource> ResourceStream { get; set; }
        public IDoProcessor<TIn, TResource> Processor { get; set; }
    }
    public class DoStreamNode<TIn, TStream, TResource> : StreamNodeBase<TIn, TStream, DoArgs<TIn, TStream, TResource>> where TStream : IStream<TIn>
    {
        public DoStreamNode(string name, DoArgs<TIn, TStream, TResource> args) : base(name, args)
        {
        }

        public override ProcessImpact PerformanceImpact => ProcessImpact.Light;

        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;

        protected override TStream CreateOutputStream(DoArgs<TIn, TStream, TResource> args)
        {
            var firstStreamWriter = args.ResourceStream.Observable;
            //if (args.PreProcess != null)
            firstStreamWriter = firstStreamWriter
                .Do(i => args.Processor.PreProcess(i))
                .DelayTillEndOfStream();
            var obs = args.Stream.Observable
                .CombineWithLatest(firstStreamWriter, (i, r) => { args.Processor.ProcessRow(i, r); return i; }, true);
            return CreateMatchingStream(obs, args.Stream);
        }
    }
    #endregion
}
