using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.SystemOld
{
    public abstract class OutputStreamNodeBase<O> : StreamNodeBase
    {
        public OutputStreamNodeBase(ExecutionContext context, string name, IEnumerable<string> parentNodeNamePath = null) : base(context, name, parentNodeNamePath)
        {
        }
        public OutputStreamNodeBase(ITracer contextual, string name, IEnumerable<string> parentNodeNamePath = null) : base(contextual.Context, name, parentNodeNamePath)
        {
        }
        protected virtual void CreateOutputStream(IObservable<O> observable)
        {
            this.Output = this.CreateStream<O>(nameof(Output), observable);
        }
        public IStream<O> Output { get; private set; }
    }
    public abstract class SortedOutputStreamNodeBase<O> : StreamNodeBase
    {
        public SortedOutputStreamNodeBase(ExecutionContext context, string name, IEnumerable<string> parentNodeNamePath = null) : base(context, name, parentNodeNamePath)
        {
        }
        public SortedOutputStreamNodeBase(ITracer contextual, string name, IEnumerable<string> parentNodeNamePath = null) : base(contextual.Context, name, parentNodeNamePath)
        {
        }
        protected virtual void CreateOutputStream(IObservable<O> observable, IEnumerable<SortCriteria<O>> sortCriterias)
        {
            this.Output = this.CreateSortedStream<O>(nameof(Output), observable, sortCriterias);
        }
        public ISortedStream<O> Output { get; private set; }
    }
    public abstract class KeyedOutputStreamNodeBase<O> : StreamNodeBase
    {
        public KeyedOutputStreamNodeBase(ExecutionContext context, string name, IEnumerable<string> parentNodeNamePath = null) : base(context, name, parentNodeNamePath)
        {
        }
        public KeyedOutputStreamNodeBase(ITracer contextual, string name, IEnumerable<string> parentNodeNamePath = null) : base(contextual.Context, name, parentNodeNamePath)
        {
        }
        protected virtual void CreateOutputStream(IObservable<O> observable, IEnumerable<SortCriteria<O>> sortCriterias)
        {
            this.Output = this.CreateKeyedStream<O>(nameof(Output), observable, sortCriterias);
        }
        public IKeyedStream<O> Output { get; private set; }
    }
    public class NodeOutput<S, I>
    {
        public NodeOutput(IStream<S> output, IStream<ErrorRow<I>> error)
        {
            this.Output = output;
            this.Error = error;
        }
        public IStream<S> Output { get; }
        public IStream<ErrorRow<I>> Error { get; }
    }
}
