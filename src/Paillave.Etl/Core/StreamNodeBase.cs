using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Paillave.Etl.Core
{

    public abstract class StreamNodeBase<TOut, TOutStream, TArgs> : INodeContext, IStreamNode<TOut, TOutStream> where TOutStream : IStream<TOut>
    {
        public Guid IdNode { get; } = Guid.NewGuid();
        public IExecutionContext ExecutionContext { get; }
        public ITraceEventFactory Tracer { get; private set; }
        public abstract ProcessImpact PerformanceImpact { get; }
        public abstract ProcessImpact MemoryFootPrint { get; }
        public string NodeName { get; }
        public virtual string TypeName
        {
            get
            {
                string name = this.GetType().Name;
                return Regex.Replace(this.GetType().Name, "StreamNode.*", "");
            }
        }
        public TOutStream Output { get; }
        protected TArgs Args { get; }

        public bool IsRootNode => false;

        public StreamNodeBase(string name, TArgs args)
        {
            // System.Diagnostics.StackFrame CallStack = new System.Diagnostics.StackFrame(3, true);
            // Console.WriteLine($"{name} at line {CallStack.GetFileName()}:{CallStack.GetFileLineNumber()}.{CallStack.GetFileColumnNumber()}");
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name), "The name of a node cannot be empty");
            }
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            this.Args = args;
            this.NodeName = name;
            this.ExecutionContext = this.GetExecutionContext(args);
            this.Tracer = new TraceEventFactory(this.ExecutionContext, this);
            this.Output = CreateOutputStream(args);
            this.Output.Observable.Subscribe(i => { }, PostProcess);
            this.ExecutionContext.AddNode(this, this.Output.Observable);
            foreach (var item in this.GetInputStreams(args))
            {
                this.ExecutionContext.AddStreamToNodeLink(new StreamToNodeLink(item.SourceNodeName, item.InputName, this.NodeName));
            }
        }
        protected virtual void PostProcess()
        {

        }
        private class InputStream
        {
            public string InputName { get; set; }
            public string SourceNodeName { get; set; }
        }
        private List<InputStream> GetInputStreams(TArgs args)
        {
            return args.GetType()
                .GetProperties()
                .Select(propertyInfo => new { propertyInfo.Name, Value = propertyInfo.GetValue(args) as IStream })
                .Where(i => i.Value != null)
                .Select(i => new InputStream { SourceNodeName = i.Value.SourceNode.NodeName, InputName = i.Name })
                .ToList();
        }

        private IExecutionContext GetExecutionContext(TArgs args)
        {
            return args.GetType()
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetValue(args))
                .OfType<IStream>()
                .Select(i => i.SourceNode.ExecutionContext)
                .OrderBy(i => !i.IsTracingContext)
                .FirstOrDefault();
        }

        protected abstract TOutStream CreateOutputStream(TArgs args);

        protected IStream<TOut> CreateUnsortedStream(IPushObservable<TOut> observable)
        {
            return new Stream<TOut>(this, observable);
        }

        protected ISingleStream<TOut> CreateSingleStream(IPushObservable<TOut> observable)
        {
            return new SingleStream<TOut>(this, observable);
        }

        protected ISortedStream<TOut, TKey> CreateSortedStream<TKey>(IPushObservable<TOut> observable, SortDefinition<TOut, TKey> sortDefinition)
        {
            return new SortedStream<TOut, TKey>(this, observable, sortDefinition);
        }

        protected IKeyedStream<TOut, TKey> CreateKeyedStream<TKey>(IPushObservable<TOut> observable, SortDefinition<TOut, TKey> sortDefinition)
        {
            return new KeyedStream<TOut, TKey>(this, observable, sortDefinition);
        }

        protected TOutStream CreateMatchingStream(IPushObservable<TOut> observable, TOutStream matchingSourceStream)
        {
            // TODO: the following is an absolute dreadful solution about which I MUST find a proper alternative
            return (TOutStream)matchingSourceStream.GetMatchingStream<TOut>(this, observable);
        }
        protected Func<T1, T2> WrapSelectForDisposal<T1, T2>(Func<T1, T2> creator, bool withNoDispose)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T2));
            if (isDisposable && !withNoDispose)
                return (T1 inp) =>
                {
                    T2 disposable = creator(inp);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return creator;
        }
        protected Func<Correlated<T1>, Correlated<T2>> WrapSelectCorrelatedForDisposal<T1, T2>(Func<Correlated<T1>, Correlated<T2>> creator, bool withNoDispose)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T2));
            if (isDisposable && !withNoDispose)
                return (Correlated<T1> inp) =>
                {
                    T2 disposable = creator(inp).Row;
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return new Correlated<T2> { Row = disposable, CorrelationKeys = inp.CorrelationKeys };
                };
            else
                return creator;
        }
        protected Func<T1, int, T2> WrapSelectIndexForDisposal<T1, T2>(Func<T1, int, T2> creator, bool withNoDispose)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T2));
            if (isDisposable && !withNoDispose)
                return (T1 inp, int index) =>
                {
                    T2 disposable = creator(inp, index);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return creator;
        }
        protected Func<T1, T2, T3> WrapSelectForDisposal<T1, T2, T3>(Func<T1, T2, T3> creator, bool withNoDispose)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T3));
            if (isDisposable && !withNoDispose)
                return (T1 inp, T2 inp2) =>
                {
                    T3 disposable = creator(inp, inp2);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return creator;
        }
        protected class IndexedObject<T>
        {
            public T Item { get; }
            public int Index { get; }
            public IndexedObject(int index, T item)
            {
                this.Index = index;
                this.Item = item;
            }
        }
        protected Func<IndexedObject<T1>, T2, T3> WrapSelectIndexObjectForDisposal<T1, T2, T3>(Func<T1, T2, int, T3> creator, bool withNoDispose)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T3));
            if (isDisposable && !withNoDispose)
                return (IndexedObject<T1> inp, T2 inp2) =>
                {
                    T3 disposable = creator(inp.Item, inp2, inp.Index);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return (IndexedObject<T1> inp, T2 inp2) => creator(inp.Item, inp2, inp.Index);
        }
        protected Func<T1, T2, int, T3> WrapSelectIndexForDisposal<T1, T2, T3>(Func<T1, T2, int, T3> creator, bool withNoDispose)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T3));
            if (isDisposable && !withNoDispose)
                return (T1 inp, T2 inp2, int index) =>
                {
                    T3 disposable = creator(inp, inp2, index);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return creator;
        }
    }
}
