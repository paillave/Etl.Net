using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;

namespace Paillave.RxPush.Operators
{
    public class DistinctUntilChangedSubject<T> : FilterSubjectBase<T>
    {
        private LambdaEqualityComparer<T> _comparer;
        private T _lastValue = default(T);
        private bool _hasLastValue = false;
        private object _syncValue = new object();
        public DistinctUntilChangedSubject(IPushObservable<T> observable, Func<T, T, bool> comparer) : base(observable)
        {
            _comparer = new LambdaEqualityComparer<T>(comparer);
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
        public static IPushObservable<T> DistinctUntilChanged<T>(this IPushObservable<T> observable, Func<T, T, bool> comparer)
        {
            return new DistinctUntilChangedSubject<T>(observable, comparer);
        }
        public static IPushObservable<T> DistinctUntilChanged<T>(this IPushObservable<T> observable) where T : IEquatable<T>
        {
            return new DistinctUntilChangedSubject<T>(observable, (l, r) => l.Equals(r));
        }
    }
}
