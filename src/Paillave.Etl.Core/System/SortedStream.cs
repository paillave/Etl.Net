using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace Paillave.Etl.Core.System
{
    public class SortedStream<T> : Stream<T>, ISortedStream<T>
    {
        private IComparer<T> _comparer;

        public SortedStream(ITracer tracer, IExecutionContext executionContext, string sourceOutputName, IObservable<T> observable, IEnumerable<SortCriteria<T>> sortCriterias) : base(tracer, executionContext, sourceOutputName, observable)
        {
            if (sortCriterias.Count() == 0) throw new ArgumentOutOfRangeException(nameof(sortCriterias), "sorting criteria list cannot be empty");
            if (tracer != null)
            {
                this.SortCriterias = new ReadOnlyCollection<SortCriteria<T>>(sortCriterias.ToList());
                this._comparer = new SortCriteriaComparer<T>(sortCriterias);
                this.Observable
                    .PairWithPrevious()
                    .Select((Pair, Index) => new { Pair, Index })
                    .Skip(1)
                    .Where(i => this._comparer.Compare(i.Pair.Item1, i.Pair.Item2) > 0)
                    .Select(i => new NotSortedStreamTraceContent(sourceOutputName, i.Index))
                    .Subscribe(tracer.Trace);
            }
        }
        public IReadOnlyCollection<SortCriteria<T>> SortCriterias { get; }
    }
}
