using System;
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

    public abstract class FilterSectionSubjectBase<TIn, TOut> : PushSubject<TOut>
    {
        protected Func<TIn, SwitchBehavior> SwitchToKeep { get; }
        protected Func<TIn, SwitchBehavior> SwitchToIgnore { get; }
        protected FilterSectionSubjectBase(IPushObservable<TIn> observable, KeepingState initialState, Func<TIn, SwitchBehavior> switchToKeep, Func<TIn, SwitchBehavior> switchToIgnore) : base(observable.CancellationToken)
        {
            this.Subscription = observable.Subscribe(HandlePushValue, this.Complete, this.PushException);
            SwitchToKeep = switchToKeep;
            SwitchToIgnore = switchToIgnore;
            CurrentState = initialState;
        }
        protected IDisposable Subscription { get; }
        private readonly object _syncValue = new object();

        protected KeepingState CurrentState { get; private set; }
        protected void SetCurrentState(KeepingState currentState)
            => CurrentState = currentState;

        protected abstract void InternalPushValue(TIn value);

        protected virtual void ProcessCurrentKeep(TIn value)
        {
            var switchRes = SwitchToIgnore(value);
            switch (switchRes)
            {
                case SwitchBehavior.SwitchIgnoreCurrent:
                    SetCurrentState(KeepingState.Ignore);
                    break;
                case SwitchBehavior.SwitchKeepCurrent:
                    SetCurrentState(KeepingState.Ignore);
                    this.InternalPushValue(value);
                    break;
                default:
                    if (CurrentState == KeepingState.Keep)
                        this.InternalPushValue(value);
                    break;
            }
        }
        protected abstract void ProcessCurrentIgnore(TIn value);
        protected void HandlePushValue(TIn value)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_syncValue)
            {
                try
                {
                    switch (CurrentState)
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
            Subscription.Dispose();
        }
    }
}