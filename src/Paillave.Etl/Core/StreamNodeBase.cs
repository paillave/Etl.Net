using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Paillave.Etl.Core;

public abstract class StreamNodeBase<TOut, TOutStream, TArgs> : INodeContext, IStreamNode<TOut, TOutStream> where TOutStream : IStream<TOut>
{
    public Guid IdNode { get; } = Guid.NewGuid();
    public IExecutionContext ExecutionContext { get; }
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
    public INodeDescription? Parent => null;
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
        var nodeContext = this.GetNodeContext(args);
        if (!string.IsNullOrWhiteSpace(nodeContext?.Parent?.NodeName))
            name = $"{nodeContext.Parent?.NodeName}>{name}";
        this.Args = args;
        this.NodeName = name;
        this.ExecutionContext = this.GetExecutionContext(args);
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
    private object GetMemberInfoValue(MemberInfo mi, object obj) => mi switch
    {
        PropertyInfo pi => pi.GetValue(obj),
        FieldInfo fi => fi.GetValue(obj),
        _ => throw new Exception("")
    };
    private IEnumerable<MemberInfo> GetMemberInfos(Type type)
        => type.GetProperties().Cast<MemberInfo>().Union(type.GetFields());
    private IEnumerable<MemberInfo> GetMemberInfos<T>() => GetMemberInfos(typeof(T));
    private List<InputStream> GetInputStreams(TArgs args)
    {
        return GetMemberInfos(args.GetType())
            .Select(propertyInfo => new { propertyInfo.Name, Value = GetMemberInfoValue(propertyInfo, args) as IStream })
            .Where(i => i.Value != null)
            .Select(i => new InputStream { SourceNodeName = i.Value.SourceNode.NodeName, InputName = i.Name })
            .ToList();
    }

    private IExecutionContext GetExecutionContext(TArgs args)
    {
        return GetMemberInfos(args.GetType())
            .Select(propertyInfo => GetMemberInfoValue(propertyInfo, args))
            .OfType<IStream>()
            .Select(i => i.SourceNode.ExecutionContext)
            .OrderBy(i => !i.IsTracingContext)
            .FirstOrDefault();
    }
    private INodeContext GetNodeContext(TArgs args)
    {
        return GetMemberInfos(args.GetType())
            .Select(propertyInfo => GetMemberInfoValue(propertyInfo, args))
            .OfType<IStream>()
            .Select(i => i.SourceNode)
            .FirstOrDefault(i => !i.ExecutionContext.IsTracingContext);
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
    protected Func<T1, T2> WrapSelectForDisposal<T1, T2>(Func<T1, IServiceProvider, T2> creator, bool withNoDispose)
    {
        bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T2));
        if (isDisposable && !withNoDispose)
            return inp =>
            {
                T2 disposable = creator(inp, this.ExecutionContext.Services);
                if (disposable is IDisposable d)
                    this.ExecutionContext.AddDisposable(d);
                return disposable;
            };
        else
            return inp => creator(inp, this.ExecutionContext.Services);
    }
    protected Func<Correlated<T1>, Correlated<T2>> WrapSelectCorrelatedForDisposal<T1, T2>(Func<Correlated<T1>, IServiceProvider, Correlated<T2>> creator, bool withNoDispose)
    {
        bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T2));
        if (isDisposable && !withNoDispose)
            return inp =>
            {
                T2 disposable = creator(inp, this.ExecutionContext.Services).Row;
                if (disposable is IDisposable d)
                    this.ExecutionContext.AddDisposable(d);
                return new Correlated<T2> { Row = disposable, CorrelationKeys = inp.CorrelationKeys };
            };
        else
            return inp => creator(inp, this.ExecutionContext.Services);
    }
    protected Func<T1, int, T2> WrapSelectIndexForDisposal<T1, T2>(Func<T1, int, IServiceProvider, T2> creator, bool withNoDispose)
    {
        bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T2));
        if (isDisposable && !withNoDispose)
            return (inp, index) =>
            {
                T2 disposable = creator(inp, index, this.ExecutionContext.Services);
                if (disposable is IDisposable d)
                    this.ExecutionContext.AddDisposable(d);
                return disposable;
            };
        else
            return (inp, idx) => creator(inp, idx, this.ExecutionContext.Services);
    }
    protected Func<T1, T2, T3> WrapSelectForDisposal<T1, T2, T3>(Func<T1, T2, IServiceProvider, T3> creator, bool withNoDispose)
    {
        bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T3));
        if (isDisposable && !withNoDispose)
            return (inp, inp2) =>
            {
                T3 disposable = creator(inp, inp2, this.ExecutionContext.Services);
                if (disposable is IDisposable d)
                    this.ExecutionContext.AddDisposable(d);
                return disposable;
            };
        else
            return (inp, inp2) => creator(inp, inp2, this.ExecutionContext.Services);
    }
    protected class IndexedObject<T>(int index, T item)
    {
        public T Item { get; } = item;
        public int Index { get; } = index;
    }
    protected Func<IndexedObject<T1>, T2, T3> WrapSelectIndexObjectForDisposal<T1, T2, T3>(Func<T1, T2, int, IServiceProvider, T3> creator, bool withNoDispose)
    {
        bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T3));
        if (isDisposable && !withNoDispose)
            return (inp, inp2) =>
            {
                T3 disposable = creator(inp.Item, inp2, inp.Index, this.ExecutionContext.Services);
                if (disposable is IDisposable d)
                    this.ExecutionContext.AddDisposable(d);
                return disposable;
            };
        else
            return (inp, inp2) => creator(inp.Item, inp2, inp.Index, this.ExecutionContext.Services);
    }
    protected Func<T1, T2, int, T3> WrapSelectIndexForDisposal<T1, T2, T3>(Func<T1, T2, int, IServiceProvider, T3> creator, bool withNoDispose)
    {
        bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(T3));
        if (isDisposable && !withNoDispose)
            return (inp, inp2, index) =>
            {
                T3 disposable = creator(inp, inp2, index, this.ExecutionContext.Services);
                if (disposable is IDisposable d)
                    this.ExecutionContext.AddDisposable(d);
                return disposable;
            };
        else
            return (i, j, idx) => creator(i, j, idx, this.ExecutionContext.Services);
    }

    public TraceEvent CreateTraceEvent(ITraceContent content, int sequenceId)
        => new(this.ExecutionContext.ExecutionId, this.TypeName, this.NodeName, content, sequenceId);
}
