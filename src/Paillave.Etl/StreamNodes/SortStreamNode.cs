using Paillave.Etl.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.Etl.Core.TraceContents;
using Paillave.Etl.Core.StreamNodesOld;
using Paillave.RxPush.Operators;

namespace Paillave.Etl.StreamNodes
{
    public class SortStreamNode<TIn> : StreamNodeBase<IStream<TIn>, TIn, IEnumerable<Core.SortCriteria<TIn>>>, ISortedStreamNodeOutput<TIn>
    {
        public ISortedStream<TIn> Output { get; }

        public SortStreamNode(IStream<TIn> input, string name, IEnumerable<Core.SortCriteria<TIn>> arguments)
            : base(input, name, arguments)
        {
            var res = input.Observable.ToList().FlatMap(i =>
            {
                i.Sort(new Core.SortCriteriaComparer<TIn>(base.Arguments));
                return PushObservable.FromEnumerable(i);
            });
            this.Output = base.CreateSortedStream(nameof(Output), res, arguments);
        }
    }
}
