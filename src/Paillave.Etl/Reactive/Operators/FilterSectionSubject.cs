using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public enum SwitchBehavior
    {
        KeepState,
        SwitchIgnoreCurrent,
        SwitchKeepCurrent
    }
    public enum KeepingState
    {
        Keep,
        Ignore
    }









    public class FilterSectionSubject<TIn, TOut> : PushSubject<TOut>
    {
        private IDisposable _subscription;
        private object _syncValue = new object();

        private int _sectionIndex = 0;

        private Func<TIn, int, TOut> _resultSelector = null;
        private Func<TIn, SwitchBehavior> _switchToKeep;
        private Func<TIn, SwitchBehavior> _switchToIgnore;
        private KeepingState _currentState;
        private FilterSectionSubject(IPushObservable<TIn> observable, Func<TIn, int, TOut> resultSelector)
        {
            _resultSelector = resultSelector;
            this._subscription = observable.Subscribe(HandlePushValue, this.Complete, this.PushException);
            _currentState = KeepingState.Ignore;
            _sectionIndex = _currentState == KeepingState.Ignore ? -1 : 0;
        }

        public FilterSectionSubject(IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switcher, Func<TIn, int, TOut> resultSelector) : this(observable, resultSelector)
        {
            _currentState = initialState;
            _switchToKeep = switcher;
            _switchToIgnore = switcher;
            _sectionIndex = _currentState == KeepingState.Ignore ? -1 : 0;
        }
        public FilterSectionSubject(IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore, Func<TIn, int, TOut> resultSelector) : this(observable, resultSelector)
        {
            _currentState = initialState;
            _switchToKeep = switchToKeep;
            _switchToIgnore = switchToIgnore;
            _currentState = KeepingState.Ignore;
            _sectionIndex = _currentState == KeepingState.Ignore ? -1 : 0;
        }
        public FilterSectionSubject(IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switcher, Func<TIn, int, TOut> resultSelector) : this(observable, resultSelector)
        {
            _currentState = KeepingState.Ignore;
            _switchToKeep = switcher;
            _switchToIgnore = switcher;
            _currentState = KeepingState.Ignore;
            _sectionIndex = _currentState == KeepingState.Ignore ? -1 : 0;
        }
        public FilterSectionSubject(IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore, Func<TIn, int, TOut> resultSelector) : this(observable, resultSelector)
        {
            _currentState = KeepingState.Ignore;
            _switchToKeep = switchToKeep;
            _switchToIgnore = switchToIgnore;
            _currentState = KeepingState.Ignore;
            _sectionIndex = _currentState == KeepingState.Ignore ? -1 : 0;
        }

        private void ProcessCurrentKeep(TIn value)
        {
            var switchRes = _switchToIgnore(value);
            switch (switchRes)
            {
                case SwitchBehavior.SwitchIgnoreCurrent:
                    _currentState = KeepingState.Ignore;
                    break;
                case SwitchBehavior.SwitchKeepCurrent:
                    _currentState = KeepingState.Ignore;
                    this.PushValue(this._resultSelector(value, _sectionIndex));
                    break;
                default:
                    if (_currentState == KeepingState.Keep)
                    {
                        this.PushValue(this._resultSelector(value, _sectionIndex));
                    }
                    break;
            }
        }
        private void ProcessCurrentIgnore(TIn value)
        {
            var switchRes = _switchToKeep(value);
            switch (switchRes)
            {
                case SwitchBehavior.SwitchIgnoreCurrent:
                    _currentState = KeepingState.Keep;
                    _sectionIndex++;
                    break;
                case SwitchBehavior.SwitchKeepCurrent:
                    _currentState = KeepingState.Keep;
                    this.PushValue(this._resultSelector(value, ++_sectionIndex));
                    break;
                default:
                    if (_currentState == KeepingState.Keep)
                    {
                        this.PushValue(this._resultSelector(value, _sectionIndex));
                    }
                    break;
            }
        }
        private void HandlePushValue(TIn value)
        {
            lock (_syncValue)
            {
                try
                {
                    switch (_currentState)
                    {
                        case KeepingState.Ignore:
                            ProcessCurrentIgnore(value);
                            break;
                        case KeepingState.Keep:
                            ProcessCurrentKeep(value);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    this.PushException(ex);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }














    public class FilterSectionSubject<TIn> : PushSubject<TIn>
    {
        private IDisposable _subscription;
        private object _syncValue = new object();

        private Func<TIn, SwitchBehavior> _switchToKeep;
        private Func<TIn, SwitchBehavior> _switchToIgnore;
        private KeepingState _currentState;
        private FilterSectionSubject(IPushObservable<TIn> observable)
        {
            this._subscription = observable.Subscribe(HandlePushValue, this.Complete, this.PushException);
            _currentState = KeepingState.Ignore;
        }

        public FilterSectionSubject(IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switcher) : this(observable)
        {
            _currentState = initialState;
            _switchToKeep = switcher;
            _switchToIgnore = switcher;
        }
        public FilterSectionSubject(IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore) : this(observable)
        {
            _currentState = initialState;
            _switchToKeep = switchToKeep;
            _switchToIgnore = switchToIgnore;
        }
        public FilterSectionSubject(IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switcher) : this(observable)
        {
            _currentState = KeepingState.Ignore;
            _switchToKeep = switcher;
            _switchToIgnore = switcher;
            _currentState = KeepingState.Ignore;
        }
        public FilterSectionSubject(IPushObservable<TIn> observable, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore) : this(observable)
        {
            _currentState = KeepingState.Ignore;
            _switchToKeep = switchToKeep;
            _switchToIgnore = switchToIgnore;
            _currentState = KeepingState.Ignore;
        }
        private void ProcessCurrentKeep(TIn value)
        {
            var switchRes = _switchToIgnore(value);
            switch (switchRes)
            {
                case SwitchBehavior.SwitchIgnoreCurrent:
                    _currentState = KeepingState.Ignore;
                    break;
                case SwitchBehavior.SwitchKeepCurrent:
                    _currentState = KeepingState.Ignore;
                    this.PushValue(value);
                    break;
                default:
                    if (_currentState == KeepingState.Keep)
                        this.PushValue(value);
                    break;
            }
        }
        private void ProcessCurrentIgnore(TIn value)
        {
            var switchRes = _switchToKeep(value);
            switch (switchRes)
            {
                case SwitchBehavior.SwitchIgnoreCurrent:
                    _currentState = KeepingState.Keep;
                    break;
                case SwitchBehavior.SwitchKeepCurrent:
                    _currentState = KeepingState.Keep;
                    this.PushValue(value);
                    break;
                default:
                    if (_currentState == KeepingState.Keep)
                        this.PushValue(value);
                    break;
            }
        }
        private void HandlePushValue(TIn value)
        {
            lock (_syncValue)
            {
                try
                {
                    switch (_currentState)
                    {
                        case KeepingState.Ignore:
                            ProcessCurrentIgnore(value);
                            break;
                        case KeepingState.Keep:
                            ProcessCurrentKeep(value);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    this.PushException(ex);
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
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
