using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;

namespace Paillave.RxPush.Operators
{
    public class FilterSubject<T> : FilterSubjectBase<T>
    {
        private Func<T, bool> _criteria;
        public FilterSubject(IPushObservable<T> observable, Func<T, bool> criteria) : base(observable)
        {
            _criteria = criteria;
        }

        protected override bool AcceptsValue(T value)
        {
            return _criteria(value);
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<T> Filter<T>(this IPushObservable<T> observable, Func<T, bool> criteria)
        {
            return new FilterSubject<T>(observable, criteria);
        }
    }
}
