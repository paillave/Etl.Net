using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;
using System.Linq.Expressions;
using Paillave.Etl.Reactive.Disposables;
using System.Threading;

namespace Paillave.Etl.Reactive.Operators
{
    public class FlatMapSubject<TIn, TOut> : FlatMapSubjectBase<TIn, TOut>
    {
        public FlatMapSubject(IPushObservable<TIn> sourceS, Func<TIn, CancellationToken, IPushObservable<TOut>> observableFactory) : base(sourceS, observableFactory) { }
        protected override IDisposableManager CreateDisposableManagerInstance() => new CollectionDisposableManager();
    }
    public class SwitchMapSubject<TIn, TOut> : FlatMapSubjectBase<TIn, TOut>
    {
        public SwitchMapSubject(IPushObservable<TIn> sourceS, Func<TIn, CancellationToken, IPushObservable<TOut>> observableFactory) : base(sourceS, observableFactory) { }
        protected override IDisposableManager CreateDisposableManagerInstance() => new SingleDisposableManager();
    }
    public abstract class FlatMapSubjectBase<TIn, TOut> : PushSubject<TOut>
    {
        private Guid _guid = Guid.NewGuid();

        private IDisposable _sourceSubscription;
        private IDisposableManager _outSubscriptions;
        private Func<TIn, CancellationToken, IPushObservable<TOut>> _observableFactory;
        private object _syncLock = new object();

        protected abstract IDisposableManager CreateDisposableManagerInstance();

        public FlatMapSubjectBase(IPushObservable<TIn> sourceS, Func<TIn, CancellationToken, IPushObservable<TOut>> observableFactory) : base(sourceS.CancellationToken)
        {
            lock (_syncLock)
            {
                _outSubscriptions = CreateDisposableManagerInstance();
                _observableFactory = observableFactory;
                _sourceSubscription = sourceS.Subscribe(OnSourcePush, OnSourceComplete, OnSourceException);
            }
        }
        private void OnSourcePush(TIn value)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_syncLock)
            {
                IPushObservable<TOut> outS;
                try
                {
                    outS = _observableFactory(value, this.CancellationToken);
                }
                catch (Exception ex)
                {
                    PushException(ex);
                    return;
                }
                //TODO: Solve the bug that makes FlatMapSubjectTests.BigLoad failing
                #region The bug is here
                IDisposable outSubscription = null;
                outSubscription = outS.Subscribe(base.PushValue, () =>
                {
                    _outSubscriptions.TryDispose(outSubscription);
                    TryComplete();
                }, base.PushException);
                var deferred = outS as IDeferredPushObservable<TOut>;
                _outSubscriptions.Set(outSubscription);
                if (deferred != null) deferred.Start();
                #endregion
            }
        }

        private void TryComplete()
        {
            if (this._sourceSubscription == null && this._outSubscriptions.IsDisposed)
            {
                base.Complete();
            }
        }

        private void OnSourceComplete()
        {
            lock (_syncLock)
            {
                this._sourceSubscription = null;
                TryComplete();
            }
        }
        private void OnSourceException(Exception ex)
        {
            base.PushException(ex);
        }

        public override void Dispose()
        {
            lock (_syncLock)
            {
                base.Dispose();
                _sourceSubscription?.Dispose();
                _outSubscriptions.Dispose();
            }
        }
    }

    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> FlatMap<TIn, TOut>(this IPushObservable<TIn> sourceS, Func<TIn, CancellationToken, IPushObservable<TOut>> observableFactory) //PERMIT TO BE SYNCHRONE
        {
            return new FlatMapSubject<TIn, TOut>(sourceS, observableFactory);
        }
        public static IPushObservable<TOut> SwitchMap<TIn, TOut>(this IPushObservable<TIn> sourceS, Func<TIn, CancellationToken, IPushObservable<TOut>> observableFactory)
        {
            return new SwitchMapSubject<TIn, TOut>(sourceS, observableFactory);
        }
    }
}
