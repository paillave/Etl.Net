using System;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class FilterSubject<T>(IPushObservable<T> observable, Func<T, bool> criteria) : FilterSubjectBase<T>(observable)
    {
        private Func<T, bool> _criteria = criteria;

        protected override bool AcceptsValue(T value) => _criteria(value);
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<T> Filter<T>(this IPushObservable<T> observable, Func<T, bool> criteria) => new FilterSubject<T>(observable, criteria);
    }
}
