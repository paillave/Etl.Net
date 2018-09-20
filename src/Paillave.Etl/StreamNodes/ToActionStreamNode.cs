using Paillave.Etl.Core.Streams;
using System;
using Paillave.Etl.Reactive.Operators;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Core;

namespace Paillave.Etl.StreamNodes
{
    public interface IToActionProcessor<TIn>
    {
        void ProcessRow(TIn value);
    }
    public class SimpleToActionProcessor<TIn> : IToActionProcessor<TIn>
    {
        private Action<TIn> _processRow;
        public SimpleToActionProcessor(Action<TIn> processRow)
        {
            _processRow = processRow;
        }
        public void ProcessRow(TIn value)
        {
            _processRow(value);
        }
    }
    public class ToActionArgs<TIn, TStream> where TStream : IStream<TIn>
    {
        public TStream Stream { get; set; }
        public IToActionProcessor<TIn> Processor { get; set; }
    }
    public class ToActionStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToActionArgs<TIn, TStream>> where TStream : IStream<TIn>
    {
        public override bool IsAwaitable => true;
        public ToActionStreamNode(string name, ToActionArgs<TIn, TStream> args) : base(name, args)
        {
        }

        protected override TStream CreateOutputStream(ToActionArgs<TIn, TStream> args)
        {
            return base.CreateMatchingStream(args.Stream.Observable.Do(args.Processor.ProcessRow), args.Stream);
        }
    }
    public class ToActionArgs<TIn, TStream, TResource> where TStream : IStream<TIn>
    {
        public TStream Stream { get; set; }
        public IStream<TResource> ResourceStream { get; set; }
        public IToActionProcessor<TIn, TResource> Processor { get; set; }
        //public Action<TIn, TResource> ProcessRow { get; set; }
        //public Action<TResource> PreProcess { get; set; }
    }
    public class ToActionStreamNode<TIn, TStream, TResource> : StreamNodeBase<TIn, TStream, ToActionArgs<TIn, TStream, TResource>> where TStream : IStream<TIn>
    {
        public override bool IsAwaitable => true;
        public ToActionStreamNode(string name, ToActionArgs<TIn, TStream, TResource> args) : base(name, args)
        {
        }
        protected override TStream CreateOutputStream(ToActionArgs<TIn, TStream, TResource> args)
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

    public interface IToActionProcessor<TIn, TResource>
    {
        void PreProcess(TResource resource);
        void ProcessRow(TIn value, TResource resource);
    }

    public class SimpleToActionProcessor<TIn, TResource> : IToActionProcessor<TIn, TResource>
    {
        private Action<TIn, TResource> _processRow;
        private Action<TResource> _preProcess;
        public SimpleToActionProcessor(Action<TIn, TResource> processRow, Action<TResource> preProcess = null)
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











    //public class ToActionArgs<TIn, TStream> where TStream : IStream<TIn>
    //{
    //    public TStream Stream { get; set; }
    //    public Action<TIn> ProcessRow { get; set; }
    //}
    //public class ToActionStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToActionArgs<TIn, TStream>> where TStream : IStream<TIn>
    //{
    //    public override bool IsAwaitable => true;
    //    public ToActionStreamNode(string name, ToActionArgs<TIn, TStream> args) : base(name, args)
    //    {
    //    }

    //    protected override TStream CreateOutputStream(ToActionArgs<TIn, TStream> args)
    //    {
    //        return base.CreateMatchingStream(args.Stream.Observable.Do(args.ProcessRow), args.Stream);
    //    }
    //}
    //public class ToActionArgs<TIn, TStream, TResource> where TStream : IStream<TIn>
    //{
    //    public TStream Stream { get; set; }
    //    public IStream<TResource> ResourceStream { get; set; }
    //    public Action<TIn, TResource> ProcessRow { get; set; }
    //    public Action<TResource> PreProcess { get; set; }
    //}
    //public class ToActionStreamNode<TIn, TStream, TResource> : StreamNodeBase<TIn, TStream, ToActionArgs<TIn, TStream, TResource>> where TStream : IStream<TIn>
    //{
    //    public override bool IsAwaitable => true;
    //    public ToActionStreamNode(string name, ToActionArgs<TIn, TStream, TResource> args) : base(name, args)
    //    {
    //    }
    //    protected override TStream CreateOutputStream(ToActionArgs<TIn, TStream, TResource> args)
    //    {
    //        var firstStreamWriter = args.ResourceStream.Observable.First();
    //        if (args.PreProcess != null) firstStreamWriter = firstStreamWriter.Do(i => args.PreProcess(i));
    //        firstStreamWriter = firstStreamWriter.DelayTillEndOfStream();
    //        var obs = args.Stream.Observable
    //            .CombineWithLatest(firstStreamWriter, (i, r) => { args.ProcessRow(i, r); return i; }, true);
    //        return CreateMatchingStream(obs, args.Stream);
    //    }
    //}
}
