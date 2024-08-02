using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class DistinctUntilChangedSubject<T> : FilterSubjectBase<T>
    {
        private IEqualityComparer<T> _comparer;
        private T _lastValue = default(T);
        private bool _hasLastValue = false;
        private object _syncValue = new object();
        public DistinctUntilChangedSubject(IPushObservable<T> observable, IEqualityComparer<T> comparer) : base(observable)
        {
            lock (_syncValue)
            {
                _comparer = comparer;
            }
        }

        protected override bool AcceptsValue(T value)
        {
            lock (_syncValue)
            {
                if (!_hasLastValue || !_comparer.Equals(_lastValue, value))
                {
                    _hasLastValue = true;
                    _lastValue = value;
                    return true;
                }
                return false;
            }
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<T> DistinctUntilChanged<T>(this IPushObservable<T> observable, IEqualityComparer<T> comparer) => new DistinctUntilChangedSubject<T>(observable, comparer);
        public static IPushObservable<T> DistinctUntilChanged<T>(this IPushObservable<T> observable, Func<T, T, bool> comparer) => new DistinctUntilChangedSubject<T>(observable, new LambdaEqualityComparer<T>(comparer));
        public static IPushObservable<T> DistinctUntilChanged<T>(this IPushObservable<T> observable) where T : IEquatable<T> => new DistinctUntilChangedSubject<T>(observable, new LambdaEqualityComparer<T>((l, r) => l.Equals(r)));
    }
}
