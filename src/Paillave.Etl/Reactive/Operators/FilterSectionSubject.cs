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
    public class FilterSectionSubject<T> : PushSubject<T>
    {
        private IDisposable _subscription;
        private object _syncValue = new object();

        private Func<T, SwitchBehavior> _switchToKeep;
        private Func<T, SwitchBehavior> _switchToIgnore;
        private KeepingState _currentState;
        private FilterSectionSubject(IPushObservable<T> observable)
        {
            this._subscription = observable.Subscribe(HandlePushValue, this.Complete, this.PushException);
        }

        public FilterSectionSubject(IPushObservable<T> observable, KeepingState initialState, Func<T, SwitchBehavior> switcher) : this(observable)
        {
            _currentState = initialState;
            _switchToKeep = switcher;
            _switchToIgnore = switcher;
        }
        public FilterSectionSubject(IPushObservable<T> observable, KeepingState initialState, Func<T, SwitchBehavior> switchToKeep, Func<T, SwitchBehavior> switchToIgnore) : this(observable)
        {
            _currentState = initialState;
            _switchToKeep = switchToKeep;
            _switchToIgnore = switchToIgnore;
        }
        public FilterSectionSubject(IPushObservable<T> observable, Func<T, SwitchBehavior> switcher) : this(observable)
        {
            _currentState = KeepingState.Ignore;
            _switchToKeep = switcher;
            _switchToIgnore = switcher;
        }
        public FilterSectionSubject(IPushObservable<T> observable, Func<T, SwitchBehavior> switchToKeep, Func<T, SwitchBehavior> switchToIgnore) : this(observable)
        {
            _currentState = KeepingState.Ignore;
            _switchToKeep = switchToKeep;
            _switchToIgnore = switchToIgnore;
        }
        private void ProcessKeep(T value)
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
        private void ProcessIgnore(T value)
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
        private void HandlePushValue(T value)
        {
            lock (_syncValue)
            {
                try
                {
                    switch (_currentState)
                    {
                        case KeepingState.Ignore:
                            ProcessIgnore(value);
                            break;
                        case KeepingState.Keep:
                            ProcessKeep(value);
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
    }
}
