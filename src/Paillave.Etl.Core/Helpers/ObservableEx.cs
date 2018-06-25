using System;
using System.Reactive.Subjects;
using System.Collections.Generic;
using System.Text;

namespace System.Reactive.Linq
{
    public static class ObservableEx
    {
        public static IObservable<T> MakeHot<T>(this IObservable<T> input)
        {
            var source = new Subject<T>();
            input.Subscribe(source.OnNext);
            return source;
        }
    }
}
