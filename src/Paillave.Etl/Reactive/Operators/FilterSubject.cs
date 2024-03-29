﻿using System;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
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
