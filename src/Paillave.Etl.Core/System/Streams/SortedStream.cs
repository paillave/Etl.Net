using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.System.TraceContents;

namespace Paillave.Etl.Core.System.Streams
{
    public class SortedStream<T> : Stream<T>, ISortedStream<T>
    {
        private IComparer<T> _comparer;

        public SortedStream(ITracer tracer, IExecutionContext executionContext, string sourceOutputName, IPushObservable<T> observable, IEnumerable<ISortCriteria<T>> sortCriterias) 
            : base(tracer, executionContext, sourceOutputName, observable)
        {
            if (sortCriterias.Count() == 0) throw new ArgumentOutOfRangeException(nameof(sortCriterias), "sorting criteria list cannot be empty");
            this.SortCriterias = new ReadOnlyCollection<ISortCriteria<T>>(sortCriterias.ToList());
            if (tracer != null)
            {
                this.SortCriterias = new ReadOnlyCollection<ISortCriteria<T>>(sortCriterias.ToList());
                this._comparer = new SortCriteriaComparer<T>(sortCriterias);
                this.Observable
                    .PairWithPrevious()
                    .Map((Pair, Index) => new { Pair, Index })
                    .Skip(1)
                    .Filter(i => this._comparer.Compare(i.Pair.Item1, i.Pair.Item2) > 0)
                    .Map(i => new NotSortedStreamTraceContent(sourceOutputName, i.Index))
                    .Subscribe(tracer.Trace);
            }
        }
        public IReadOnlyCollection<ISortCriteria<T>> SortCriterias { get; }
    }
}
