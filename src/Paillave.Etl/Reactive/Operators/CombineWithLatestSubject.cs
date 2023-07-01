using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Paillave.Etl.Reactive.Operators
{
    public class CombineWithLatestSubject<TIn1, TIn2, TOut> : PushSubject<TOut>
    {
        private readonly object _lockObject = new object();
        private readonly ObservableElement<TIn1> _obsel1;
        private readonly ObservableElement<TIn2> _obsel2;
        private readonly Func<TIn1, TIn2, TOut> _selector;
        private readonly bool _bufferTillFirstMatch;
        public CombineWithLatestSubject(IPushObservable<TIn1> observable1, IPushObservable<TIn2> observable2, Func<TIn1, TIn2, TOut> selector, bool bufferTillFirstMatch = false) : base(CancellationTokenSource.CreateLinkedTokenSource(observable1.CancellationToken, observable2.CancellationToken).Token)
        {
            lock (_lockObject)
            {
                _bufferTillFirstMatch = bufferTillFirstMatch;
                _selector = selector;
                var disp1 = observable1.Subscribe(HandlePushValue1, HandleComplete1, this.PushException);
                this._obsel1 = new ObservableElement<TIn1>(disp1);
                var disp2 = observable2.Subscribe(HandlePushValue2, HandleComplete2, this.PushException);
                this._obsel2 = new ObservableElement<TIn2>(disp2);
            }
        }
        private void HandlePushValue1(TIn1 value)
        {
            lock (_lockObject)
            {
                _obsel1.LastValue = value;
                TryPushCombination(1);
            }
        }

        private void HandlePushValue2(TIn2 value)
        {
            lock (_lockObject)
            {
                _obsel2.LastValue = value;
                TryPushCombination(2);
            }
        }
        private void TryPushCombination(int inputNumber)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_lockObject)
            {
                if (_obsel1.HasLastValue && _obsel2.HasLastValue)
                {
                    if (_bufferTillFirstMatch)
                    {
                        if (inputNumber == 1 && _obsel2.Buffer.Any())
                        {
                            while (_obsel2.Buffer.Any())
                            {
                                if (this.CancellationToken.IsCancellationRequested)
                                {
                                    break;
                                }
                                PushValues(_obsel1.LastValue, _obsel2.Buffer.Dequeue());
                            }
                            _obsel1.Buffer.Clear();
                        }
                        else if (inputNumber == 2 && _obsel1.Buffer.Any())
                        {
                            while (_obsel1.Buffer.Any())
                            {
                                if (this.CancellationToken.IsCancellationRequested)
                                {
                                    break;
                                }
                                PushValues(_obsel1.Buffer.Dequeue(), _obsel2.LastValue);
                            }
                            _obsel2.Buffer.Clear();
                        }
                        else
                            PushValues(_obsel1.LastValue, _obsel2.LastValue);
                    }
                    else
                        PushValues(_obsel1.LastValue, _obsel2.LastValue);
                }
                else
                {
                    if (_bufferTillFirstMatch)
                    {
                        if (!_obsel1.HasLastValue && _obsel2.HasLastValue) _obsel2.Buffer.Enqueue(_obsel2.LastValue);
                        else if (!_obsel2.HasLastValue && _obsel1.HasLastValue) _obsel1.Buffer.Enqueue(_obsel1.LastValue);
                    }
                }
            }
        }
        private void PushValues(TIn1 in1, TIn2 in2)
        {
            TOut ret;
            try
            {
                ret = _selector(in1, in2);
                PushValue(ret);
            }
            catch (Exception ex)
            {
                PushException(ex);
            }
        }
        private void HandleComplete1()
        {
            lock (_lockObject)
            {
                if (_obsel1 != null)
                    _obsel1.IsComplete = true;
                TryComplete();
            }
        }
        private void HandleComplete2()
        {
            lock (_lockObject)
            {
                if (_obsel2 != null)
                    _obsel2.IsComplete = true;
                TryComplete();
            }
        }
        private void TryComplete()
        {
            if (((_obsel1?.IsComplete) ?? false) && ((_obsel2?.IsComplete) ?? false))
                Complete();
        }
        public override void Dispose()
        {
            _obsel1.Dispose();
            _obsel2.Dispose();
            base.Dispose();
        }
        private class ObservableElement<T> : IDisposable
        {
            private IDisposable _disposable;
            private T _lastValue = default;
            public Queue<T> Buffer = new Queue<T>();
            public T LastValue
            {
                get { return _lastValue; }
                set
                {
                    HasLastValue = true;
                    _lastValue = value;
                }
            }
            public bool IsComplete { get; set; }
            public bool HasLastValue { get; private set; } = false;

            public ObservableElement(IDisposable disposable)
            {
                _disposable = disposable;
            }
            public void Dispose()
            {
                _disposable.Dispose();
            }
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<TOut> CombineWithLatest<TIn1, TIn2, TOut>(this IPushObservable<TIn1> observable1, IPushObservable<TIn2> observable2, Func<TIn1, TIn2, TOut> selector, bool bufferTillFirstMatch = false)
        {
            return new CombineWithLatestSubject<TIn1, TIn2, TOut>(observable1, observable2, selector, bufferTillFirstMatch);
        }
    }
}
