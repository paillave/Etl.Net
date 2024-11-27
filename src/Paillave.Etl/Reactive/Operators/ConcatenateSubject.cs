using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Paillave.Etl.Reactive.Operators
{
    public class ConcatenateSubject<TIn> : PushSubject<TIn>
    {
        private IDisposable _topSubscription;
        private IDisposable _bottomSubscription;
        private object _lockObject = new object();
        private List<TIn> _bottomBuffer = new List<TIn>();
        private bool _topComplete = false;
        private bool _bottomComplete = false;

        public ConcatenateSubject(IPushObservable<TIn> topObservable, IPushObservable<TIn> bottomObservable) : base(CancellationTokenSource.CreateLinkedTokenSource(topObservable.CancellationToken, bottomObservable.CancellationToken).Token)
        {
            _topSubscription = topObservable.Subscribe(this.HandlePushTop, this.HandleCompleteTop, this.HandleException);
            _bottomSubscription = bottomObservable.Subscribe(this.HandlePushBottom, this.HandleCompleteBottom, this.HandleException);
        }

        private void HandleCompleteBottom()
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_lockObject)
            {
                _bottomComplete = true;
                if (this._topComplete)
                    this.Complete();
            }
        }

        private void HandlePushBottom(TIn obj)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_lockObject)
            {
                if (_topComplete)
                    PushValue(obj);
                else
                    _bottomBuffer.Add(obj);
            }
        }

        private void HandleException(Exception ex)
        {
            lock (_lockObject)
                base.PushException(ex);
        }

        private void HandleCompleteTop()
        {
            lock (_lockObject)
            {
                _topComplete = true;
                foreach (var item in _bottomBuffer)
                {
                    if (CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                    this.PushValue(item);
                }
                _bottomBuffer.Clear();
                if (_bottomComplete)
                    this.Complete();
            }
        }

        private void HandlePushTop(TIn obj)
        {
            if (CancellationToken.IsCancellationRequested)
            {
                return;
            }
            lock (_lockObject)
            {
                this.PushValue(obj);
            }
        }
        public override void Dispose()
        {
            _topSubscription.Dispose();
            _bottomSubscription.Dispose();
        }
    }
    public static partial class ObservableExtensions
    {
        public static IPushObservable<T> Concatenate<T>(this IPushObservable<T> observable, IPushObservable<T> bottomPart) => new ConcatenateSubject<T>(observable, bottomPart);
    }
}
