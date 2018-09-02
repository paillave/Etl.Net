using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class DistinctSubject<T> : FilterSubjectBase<T>
    {
        private IEqualityComparer<T> _comparer;
        private IList<T> _passedItems = new List<T>();
        private object _syncValue = new object();
        public DistinctSubject(IPushObservable<T> observable, IEqualityComparer<T> comparer) : base(observable)
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
                if (!_passedItems.Contains(value, _comparer))
                {
                    _passedItems.Add(value);
                    return true;
                }
                return false;
            }
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<T> Distinct<T>(this IPushObservable<T> observable, IEqualityComparer<T> comparer)
        {
            return new DistinctSubject<T>(observable, comparer);
        }
        public static IPushObservable<T> Distinct<T>(this IPushObservable<T> observable, Func<T, T, bool> comparer)
        {
            return new DistinctSubject<T>(observable, new LambdaEqualityComparer<T>(comparer));
        }
        public static IPushObservable<T> Distinct<T>(this IPushObservable<T> observable) where T : IEquatable<T>
        {
            return new DistinctSubject<T>(observable, new LambdaEqualityComparer<T>((l, r) => l.Equals(r)));
        }
    }
}
