using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class FilterSectionSubject<TIn, TOut> : FilterSectionSubjectBase<TIn, TOut>
    {
        private int _sectionIndex = 0;

        private readonly Func<TIn, int, TOut> _resultSelector = null;
        public FilterSectionSubject(IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore, Func<TIn, int, TOut> resultSelector)
            : base(observable, initialState, switchToKeep, switchToIgnore)
        {
            _resultSelector = resultSelector;
            _sectionIndex = initialState == KeepingState.Ignore ? -1 : 0;
        }

        public FilterSectionSubject(IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switcher, Func<TIn, int, TOut> resultSelector)
            : this(observable, initialState, switcher, switcher, resultSelector) { }
        public FilterSectionSubject(IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switcher, Func<TIn, int, TOut> resultSelector)
            : this(observable, KeepingState.Ignore, switcher, switcher, resultSelector) { }
        public FilterSectionSubject(IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore, Func<TIn, int, TOut> resultSelector)
            : this(observable, KeepingState.Ignore, switchToKeep, switchToIgnore, resultSelector) { }

        protected override void InternalPushValue(TIn value)
        {
            this.TryPushValue(() => this._resultSelector(value, _sectionIndex));
        }
        protected override void ProcessCurrentIgnore(TIn value)
        {
            var switchRes = SwitchToKeep(value);
            switch (switchRes)
            {
                case SwitchBehavior.SwitchIgnoreCurrent:
                    SetCurrentState(KeepingState.Keep);
                    _sectionIndex++;
                    break;
                case SwitchBehavior.SwitchKeepCurrent:
                    SetCurrentState(KeepingState.Keep);
                    ++_sectionIndex;
                    this.InternalPushValue(value);
                    break;
                default:
                    if (CurrentState == KeepingState.Keep)
                        this.InternalPushValue(value);
                    break;
            }
        }
    }














    public class FilterSectionSubject<TIn> : FilterSectionSubjectBase<TIn, TIn>
    {
        public FilterSectionSubject(IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switcher)
            : base(observable, initialState, switcher, switcher) { }
        public FilterSectionSubject(IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore)
            : base(observable, initialState, switchToKeep, switchToIgnore) { }
        public FilterSectionSubject(IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switcher)
            : base(observable, KeepingState.Ignore, switcher, switcher) { }
        public FilterSectionSubject(IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore)
            : base(observable, KeepingState.Ignore, switchToKeep, switchToIgnore) { }
        protected override void InternalPushValue(TIn value)
        {
            this.PushValue(value);
        }
        protected override void ProcessCurrentIgnore(TIn value)
        {
            var switchRes = SwitchToKeep(value);
            switch (switchRes)
            {
                case SwitchBehavior.SwitchIgnoreCurrent:
                    SetCurrentState(KeepingState.Keep);
                    break;
                case SwitchBehavior.SwitchKeepCurrent:
                    SetCurrentState(KeepingState.Keep);
                    this.PushValue(value);
                    break;
                default:
                    if (CurrentState == KeepingState.Keep)
                        this.PushValue(value);
                    break;
            }
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<T> FilterSection<T>(this IPushObservable<T> observable, KeepingState initialState, Func<T, SwitchBehavior> switcher)
        {
            return new FilterSectionSubject<T>(observable, initialState, switcher);
        }
        public static IPushObservable<T> FilterSection<T>(this IPushObservable<T> observable, KeepingState initialState, Func<T, SwitchBehavior> switchToKeep, Func<T, SwitchBehavior> switchToIgnore)
        {
            return new FilterSectionSubject<T>(observable, initialState, switchToKeep, switchToIgnore);
        }
        public static IPushObservable<T> FilterSection<T>(this IPushObservable<T> observable, Func<T, SwitchBehavior> switcher)
        {
            return new FilterSectionSubject<T>(observable, switcher);
        }
        public static IPushObservable<T> FilterSection<T>(this IPushObservable<T> observable, Func<T, SwitchBehavior> switchToKeep, Func<T, SwitchBehavior> switchToIgnore)
        {
            return new FilterSectionSubject<T>(observable, switchToKeep, switchToIgnore);
        }

        public static IPushObservable<TOut> FilterSection<TIn, TOut>(this IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switcher, Func<TIn, int, TOut> resultSelector)
        {
            return new FilterSectionSubject<TIn, TOut>(observable, initialState, switcher, resultSelector);
        }
        public static IPushObservable<TOut> FilterSection<TIn, TOut>(this IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore, Func<TIn, int, TOut> resultSelector)
        {
            return new FilterSectionSubject<TIn, TOut>(observable, initialState, switchToKeep, switchToIgnore, resultSelector);
        }
        public static IPushObservable<TOut> FilterSection<TIn, TOut>(this IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switcher, Func<TIn, int, TOut> resultSelector)
        {
            return new FilterSectionSubject<TIn, TOut>(observable, switcher, resultSelector);
        }
        public static IPushObservable<TOut> FilterSection<TIn, TOut>(this IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore, Func<TIn, int, TOut> resultSelector)
        {
            return new FilterSectionSubject<TIn, TOut>(observable, switchToKeep, switchToIgnore, resultSelector);
        }
    }
}
