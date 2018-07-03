using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace Paillave.Etl.Core.SystemOld
{
    public class SortedStream<T> : Stream<T>, ISortedStream<T>
    {
        private IComparer<T> _comparer;
        public SortedStream(ITracer tracer, IEnumerable<string> sourceNodeName, string sourceOutputName, IObservable<T> observable, IEnumerable<SortCriteria<T>> sortCriterias) : base(traceContext, sourceNodeName, sourceOutputName, observable)
        {
            if (sortCriterias.Count() == 0) throw new ArgumentOutOfRangeException(nameof(sortCriterias), "sorting criteria list cannot be empty");
            if (tracer != null)
            {
                this.SortCriterias = new ReadOnlyCollection<SortCriteria<T>>(sortCriterias.ToList());
                this._comparer = new SortCriteriaComparer<T>(sortCriterias);
                observable
                    .PairWithPrevious()
                    .Select((Pair, Index) => new { Pair, Index })
                    .Skip(1)
                    .Where(i => this._comparer.Compare(i.Pair.Item1, i.Pair.Item2) > 0)
                    .Select(i => new NotSortedStreamTraceContent(this.SourceNodeName, this.SourceOutputName, i.Index))
                    .Subscribe(tracer.OnNextProcessTrace);
            }
        }
        public IReadOnlyCollection<SortCriteria<T>> SortCriterias { get; private set; }
    }
}
