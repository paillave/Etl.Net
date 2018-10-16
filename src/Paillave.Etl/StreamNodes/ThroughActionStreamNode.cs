using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.Reactive.Operators;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Core;

namespace Paillave.Etl.StreamNodes
{
    #region Actions Without Resources
    public interface IThroughActionProcessor<TIn>
    {
        void ProcessRow(TIn value);
    }
    public class SimpleThroughActionProcessor<TIn> : IThroughActionProcessor<TIn>
    {
        private Action<TIn> _processRow;
        public SimpleThroughActionProcessor(Action<TIn> processRow)
        {
            _processRow = processRow;
        }
        public void ProcessRow(TIn value)
        {
            _processRow(value);
        }
    }
    public class ContextThroughActionProcessor<TIn, TCtx> : IThroughActionProcessor<TIn>
    {
        private Action<TIn, TCtx, Action<TCtx>> _processRow;
        private TCtx _context;
        public ContextThroughActionProcessor(Action<TIn, TCtx, Action<TCtx>> processRow, TCtx initialContext)
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
    public class ThroughActionArgs<TIn, TStream> where TStream : IStream<TIn>
    {
        public TStream Stream { get; set; }
        public IThroughActionProcessor<TIn> Processor { get; set; }
    }
    public class ThroughActionStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ThroughActionArgs<TIn, TStream>> where TStream : IStream<TIn>
    {
        public override bool IsAwaitable => true;
        public ThroughActionStreamNode(string name, ThroughActionArgs<TIn, TStream> args) : base(name, args)
        {
        }

        protected override TStream CreateOutputStream(ThroughActionArgs<TIn, TStream> args)
        {
            return base.CreateMatchingStream(args.Stream.Observable.Do(args.Processor.ProcessRow), args.Stream);
        }
    }
    #endregion

    #region Actions With Resources
    public interface IThroughActionProcessor<TIn, TResource>
    {
        void PreProcess(TResource resource);
        void ProcessRow(TIn value, TResource resource);
    }
    public class SimpleThroughActionProcessor<TIn, TResource> : IThroughActionProcessor<TIn, TResource>
    {
        private Action<TIn, TResource> _processRow;
        private Action<TResource> _preProcess;
        public SimpleThroughActionProcessor(Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
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
    public class ContextThroughActionProcessor<TIn, TResource, TCtx> : IThroughActionProcessor<TIn, TResource>
    {
        private Action<TIn, TResource, TCtx, Action<TCtx>> _processRow;
        private Action<TResource, Action<TCtx>> _preProcess;
        private TCtx _context;
        public ContextThroughActionProcessor(Action<TIn, TResource, TCtx, Action<TCtx>> processRow, Action<TResource, Action<TCtx>> preProcess = null)
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
    public class ThroughActionArgs<TIn, TStream, TResource> where TStream : IStream<TIn>
    {
        public TStream Stream { get; set; }
        public ISingleStream<TResource> ResourceStream { get; set; }
        public IThroughActionProcessor<TIn, TResource> Processor { get; set; }
    }
    public class ThroughActionStreamNode<TIn, TStream, TResource> : StreamNodeBase<TIn, TStream, ThroughActionArgs<TIn, TStream, TResource>> where TStream : IStream<TIn>
    {
        public override bool IsAwaitable => true;
        public ThroughActionStreamNode(string name, ThroughActionArgs<TIn, TStream, TResource> args) : base(name, args)
        {
        }
        protected override TStream CreateOutputStream(ThroughActionArgs<TIn, TStream, TResource> args)
        {
            var firstStreamWriter = args.ResourceStream.Observable.First();
            //if (args.PreProcess != null)
            firstStreamWriter = firstStreamWriter.Do(i => args.Processor.PreProcess(i));
            firstStreamWriter = firstStreamWriter.DelayTillEndOfStream();
            var obs = args.Stream.Observable
                .CombineWithLatest(firstStreamWriter, (i, r) => { args.Processor.ProcessRow(i, r); return i; }, true);
            return CreateMatchingStream(obs, args.Stream);
        }
    }
    #endregion
}
