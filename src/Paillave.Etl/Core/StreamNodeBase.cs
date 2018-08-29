using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.Core
{
    public abstract class StreamNodeBase<TOut, TOutStream, TArgs> : INodeContext
        where TOutStream : IStream<TOut>
    {
        protected IExecutionContext ExecutionContext { get; }
        protected ITracer Tracer { get; private set; }
        public string NodeName { get; private set; }
        public virtual string TypeName { get; private set; }
        public virtual bool IsAwaitable { get; } = false;
        public TOutStream Output { get; }

        public StreamNodeBase(string name, TArgs args)
        {
            this.TypeName = this.GetType().Name;
            this.NodeName = name;
            this.ExecutionContext = this.GetExecutionContext(args);
            this.Tracer = new Tracer(this.ExecutionContext, this);
            this.Output = CreateOutputStream(args);
            if (this.IsAwaitable)
                this.ExecutionContext.AddToWaitForCompletion(name, this.Output.Observable);
            foreach (var item in this.GetInputStreams(args))
                this.ExecutionContext.AddStreamToNodeLink(new StreamToNodeLink(item.SourceNodeName, item.InputName, this.NodeName));
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
                .Select(i => new InputStream { SourceNodeName = i.Value.SourceNodeName, InputName = i.Name })
                .ToList();
        }

        private IExecutionContext GetExecutionContext(TArgs args)
        {
            return args.GetType()
                .GetProperties()
                .Select(propertyInfo => propertyInfo.GetValue(args))
                .OfType<IStream>()
                .Select(i => i.ExecutionContext)
                .OrderBy(i => !i.IsTracingContext)
                .FirstOrDefault();
        }

        protected abstract TOutStream CreateOutputStream(TArgs args);

        protected IStream<TOut> CreateStream(IPushObservable<TOut> observable)
        {
            return new Stream<TOut>(this.Tracer, this.ExecutionContext, this.NodeName, observable);
        }

        protected ISortedStream<TOut, TKey> CreateSortedStream<TKey>(IPushObservable<TOut> observable, SortDefinition<TOut, TKey> sortDefinition)
        {
            return new SortedStream<TOut, TKey>(this.Tracer, this.ExecutionContext, this.NodeName, observable, sortDefinition);
        }

        protected IKeyedStream<TOut, TKey> CreateKeyedStream<TKey>(IPushObservable<TOut> observable, SortDefinition<TOut, TKey> sortDefinition)
        {
            return new KeyedStream<TOut,TKey>(this.Tracer, this.ExecutionContext, this.NodeName, observable, sortDefinition);
        }

        protected TOutStream CreateMatchingStream(IPushObservable<TOut> observable, TOutStream matchingSourceStream)
        {
            //IS THERE ANY WAY TO GET RID OF THESE HORRIFYING DOUBLE CASTS?
            switch (matchingSourceStream)
            {
                case IKeyedStream<TOut> ks:
                    return (TOutStream)(object)new KeyedStream<TOut>(Tracer, ExecutionContext, NodeName, observable, ks.SortCriterias);
                case ISortedStream<TOut> ss:
                    return (TOutStream)(object)new SortedStream<TOut>(Tracer, ExecutionContext, NodeName, observable, ss.SortCriterias);
            }
            return (TOutStream)(object)new Stream<TOut>(Tracer, ExecutionContext, NodeName, observable);
        }
        protected Func<T1, T2> WrapSelectForDisposal<T1, T2>(Func<T1, T2> creator)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T2));
            if (isDisposable)
                return (T1 inp) =>
                {
                    T2 disposable = creator(inp);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return creator;
        }
        protected Func<T1, int, T2> WrapSelectIndexForDisposal<T1, T2>(Func<T1, int, T2> creator)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T2));
            if (isDisposable)
                return (T1 inp, int index) =>
                {
                    T2 disposable = creator(inp, index);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return creator;
        }
        protected Func<T1, T2, T3> WrapSelectForDisposal<T1, T2, T3>(Func<T1, T2, T3> creator)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T3));
            if (isDisposable)
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
        protected Func<IndexedObject<T1>, T2, T3> WrapSelectIndexObjectForDisposal<T1, T2, T3>(Func<T1, T2, int, T3> creator)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T3));
            if (isDisposable)
                return (IndexedObject<T1> inp, T2 inp2) =>
                {
                    T3 disposable = creator(inp.Item, inp2, inp.Index);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return (IndexedObject<T1> inp, T2 inp2) => creator(inp.Item, inp2, inp.Index);
        }
        protected Func<T1, T2, int, T3> WrapSelectIndexForDisposal<T1, T2, T3>(Func<T1, T2, int, T3> creator)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T3));
            if (isDisposable)
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
