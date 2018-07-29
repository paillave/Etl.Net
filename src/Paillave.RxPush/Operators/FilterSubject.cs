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
    public abstract class FilterSubjectBase<T> : PushSubject<T>
    {
        private IDisposable _subscription;

        protected abstract bool AcceptsValue(T value);

        public FilterSubjectBase(IPushObservable<T> observable)
        {
            this._subscription = observable.Subscribe(i =>
            {
                try
                {
                    if (AcceptsValue(i))
                        this.PushValue(i);
                }
                catch (Exception ex)
                {
                    this.PushException(ex);
                }
            }, this.Complete, this.PushException);
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
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
