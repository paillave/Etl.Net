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
    public class SortedStream<T> : Stream<T>
    {
        private IComparer<T> _comparer;
        public SortedStream(ExecutionContextBase traceContext, string sourceNodeName, string sourceOutputName, IObservable<T> observable, params SortCriteria<T>[] sortCriterias) : base(traceContext, sourceNodeName, sourceOutputName, observable)
        {
            this._comparer = new SortCriteriaComparer<T>(sortCriterias);
            this.ObservableCopyIfUsed
                .Select((Value, Index) => new { Value, Index })
                .Scan(new { Previous = default(T), Index = 0, Comparison = 0 }, (a, v) => new { Previous = v.Value, Index = v.Index, Comparison = _comparer.Compare(a.Previous, v.Value) })
                .Skip(1)
                .Where(i => i.Comparison >= 0)
                .Subscribe(i => this.TraceSubject.OnNext(new NotSortedStreamProcessTrace(this.SourceNodeName, this.SourceOutputName, i.Index)));
            if (sortCriterias.Length == 0) throw new ArgumentOutOfRangeException(nameof(sortCriterias), "sorting criteria list cannot be empty");
            this.SortCriterias = new ReadOnlyCollection<SortCriteria<T>>(sortCriterias.ToList());
        }
        public IReadOnlyCollection<SortCriteria<T>> SortCriterias { get; private set; }
    }
}
