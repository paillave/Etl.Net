﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Paillave.Etl.Reactive.Core;

namespace Paillave.Etl.Reactive.Operators
{
    public class CompletesOnExceptionSubject<T, E> : PushSubject<T> where E : Exception
    {
        private IDisposable _subscription;
        private object lockObject = new object();

        public CompletesOnExceptionSubject(IPushObservable<T> observable, Action<Exception> catchMethod) : base(observable.CancellationToken)
        {
            lock (lockObject)
            {
                this._subscription = observable.Subscribe(this.PushValue, this.Complete, ex =>
                {
                    lock (lockObject)
                    {
                        if (ex is E)
                        {
                            catchMethod(ex);
                            this.Complete();
                        }
                    }
                });
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
        public static IPushObservable<T> CompletesOnException<T>(this IPushObservable<T> observable, Action<Exception> catchMethod) => new CompletesOnExceptionSubject<T, Exception>(observable, catchMethod);
        public static IPushObservable<T> CompletesOnException<T>(this IPushObservable<T> observable) => new CompletesOnExceptionSubject<T, Exception>(observable, _ => { });
        public static IPushObservable<T> CompletesOnException<T, E>(this IPushObservable<T> observable, Action<Exception> catchMethod) where E : Exception => new CompletesOnExceptionSubject<T, E>(observable, catchMethod);
        public static IPushObservable<T> CompletesOnException<T, E>(this IPushObservable<T> observable) where E : Exception => new CompletesOnExceptionSubject<T, E>(observable, _ => { });
    }
}
