using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.RxPush.Core;
using System.Linq.Expressions;
using Paillave.RxPush.Disposables;

namespace Paillave.RxPush.Operators
{
    public class FlatMapSubject<TIn, TOut> : FlatMapSubjectBase<TIn, TOut>
    {
        public FlatMapSubject(IPushObservable<TIn> sourceS, Func<TIn, IPushObservable<TOut>> observableFactory) : base(sourceS, observableFactory) { }
        protected override IDisposableManager CreateDisposableManagerInstance() => new CollectionDisposableManager();
    }
    public class SwitchMapSubject<TIn, TOut> : FlatMapSubjectBase<TIn, TOut>
    {
        public SwitchMapSubject(IPushObservable<TIn> sourceS, Func<TIn, IPushObservable<TOut>> observableFactory) : base(sourceS, observableFactory) { }
        protected override IDisposableManager CreateDisposableManagerInstance() => new SingleDisposableManager();
    }
    public abstract class FlatMapSubjectBase<TIn, TOut> : PushSubject<TOut>
    {
        private IDisposable _sourceSubscription;
        private IDisposableManager _outSubscriptions;
        private Func<TIn, IPushObservable<TOut>> _observableFactory;
        private object syncLock = new object();

        protected abstract IDisposableManager CreateDisposableManagerInstance();

        public FlatMapSubjectBase(IPushObservable<TIn> sourceS, Func<TIn, IPushObservable<TOut>> observableFactory)
        {
            _outSubscriptions = CreateDisposableManagerInstance();
            _observableFactory = observableFactory;
            _sourceSubscription = sourceS.Subscribe(OnSourcePush, OnSourceComplete, OnSourceException);
        }
        private void OnSourcePush(TIn value)
        {
            lock (syncLock)
            {
                IPushObservable<TOut> outS;
                try
                {
                    outS = _observableFactory(value);
                }
                catch (Exception ex)
                {
                    PushException(ex);
                    return;
                }
                IDisposable outSubscription = null;
                outSubscription = outS.Subscribe(base.PushValue, () =>
                {
                    _outSubscriptions.TryDispose(outSubscription);
                    TryComplete();
                }, base.PushException);
                _outSubscriptions.Set(outSubscription);
            }
        }

        private void TryComplete()
        {
            if (this._sourceSubscription == null && this._outSubscriptions.IsDisposed)
                base.Complete();
        }

        private void OnSourceComplete()
        {
            lock (syncLock)
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
            lock (syncLock)
            {
                base.Dispose();
                _sourceSubscription?.Dispose();
                _outSubscriptions.Dispose();
            }
        }
    }

    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> FlatMap<TIn, TOut>(this IPushObservable<TIn> sourceS, Func<TIn, IPushObservable<TOut>> observableFactory)
        {
            return new FlatMapSubject<TIn, TOut>(sourceS, observableFactory);
        }
        public static IPushObservable<TOut> SwitchMap<TIn, TOut>(this IPushObservable<TIn> sourceS, Func<TIn, IPushObservable<TOut>> observableFactory)
        {
            return new SwitchMapSubject<TIn, TOut>(sourceS, observableFactory);
        }
    }
}
