using Paillave.Etl.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{
    public class SortStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, IEnumerable<Core.SortCriteria<TIn>>>, ISortedStreamNodeOutput<TIn>
    {
        private object _syncObject = new object();
        public ISortedStream<TIn> Output { get; }
        private DeferedPushObservable<TIn> _deferedPushObservable;
        private List<TIn> _items = new List<TIn>();

        private void PushSortedList(Action<TIn> pushValue)
        {
            foreach (var item in _items)
                pushValue(item);
        }

        public SortStreamNode(IStream<TIn> input, string name, IEnumerable<string> parentNodeNamePath, IEnumerable<Core.SortCriteria<TIn>> arguments)
            : base(input, name, parentNodeNamePath, arguments)
        {
            _deferedPushObservable = new DeferedPushObservable<TIn>(PushSortedList);
            input.Observable.Subscribe(HandlePush, HandleComplete);
            this.Output = base.CreateSortedStream(nameof(Output), _deferedPushObservable, arguments);
        }

        private void HandleComplete()
        {
            lock (_syncObject)
            {
                _items.Sort(new Core.SortCriteriaComparer<TIn>(base.Arguments));
                _deferedPushObservable.Start();
            }
        }

        private void HandlePush(TIn value)
        {
            lock (_syncObject)
            {
                if (_items.Count == 10000)
                    base.Tracer.Trace(new SortWarningStreamTraceContent(nameof(this.Output)));
                _items.Add(value);
            }
        }
    }
    public static partial class StreamEx
    {
        public static ISortedStream<TIn> Sort<TIn>(this IStream<TIn> stream, string name, params Expression<Func<TIn, IComparable>>[] sortFields)
        {
            return new SortStreamNode<TIn>(stream, name, null, sortFields.Select(i => new Core.SortCriteria<TIn>(i)).ToList()).Output;
        }
        public static ISortedStream<TIn> Sort<TIn>(this IStream<TIn> stream, string name, params Core.SortCriteria<TIn>[] sortCriterias)
        {
            return new SortStreamNode<TIn>(stream, name, null, sortCriterias).Output;
        }
        public static ISortedStream<TIn> Sort<TIn>(this IStream<TIn> stream, string name, Func<TIn, IEnumerable<Core.SortCriteria<TIn>>> sortCriteriasBuilder)
        {
            return new SortStreamNode<TIn>(stream, name, null, sortCriteriasBuilder(default(TIn)).ToList()).Output;
        }
        public static ISortedStream<TIn> Sort<TIn>(this IStream<TIn> stream, string name, Func<TIn, Core.SortCriteria<TIn>> sortCriteriasBuilder)
        {
            return new SortStreamNode<TIn>(stream, name, null, new[] { sortCriteriasBuilder(default(TIn)) }).Output;
        }
    }
}
