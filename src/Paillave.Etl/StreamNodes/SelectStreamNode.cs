using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.NodeOutputs;
using Paillave.Etl.Core.StreamNodes;

namespace Paillave.Etl.StreamNodes
{
    public class SelectArgs<TIn, TOut>
    {
        public Func<TIn, TOut> Mapper { get; set; } = null;
        public Func<TIn, int, TOut> IndexMapper { get; set; } = null;
        public bool RedirectErrorsInsteadOfFail { get; set; }
    }
    public class SelectStreamNode<TIn, TOut> : StreamNodeBase<IStream<TIn>, TIn, SelectArgs<TIn, TOut>>, IStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn>>
    {
        public SelectStreamNode(IStream<TIn> input, string name, SelectArgs<TIn, TOut> arguments) : base(input, name, arguments)
        {
            bool isDisposable = typeof(IDisposable).IsAssignableFrom(typeof(TOut));
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = arguments.Mapper == null ?
                    input.Observable.Map(base.ErrorManagementWrapFunction(WrapSelect(isDisposable, arguments.IndexMapper)))
                    : input.Observable.Map(base.ErrorManagementWrapFunction(WrapSelect(isDisposable, arguments.Mapper)));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Map(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TIn>(i)));
            }
            else
                this.Output = arguments.Mapper == null ?
                    base.CreateStream(nameof(this.Output), input.Observable.Map(WrapSelect(isDisposable, arguments.IndexMapper)))
                    : base.CreateStream(nameof(this.Output), input.Observable.Map(WrapSelect(isDisposable, arguments.Mapper)));

            //Observable.Create()
        }

        private Func<TIn, TOut> WrapSelect(bool isDisposable, Func<TIn, TOut> creator)
        {
            if (isDisposable)
                return (TIn inp) =>
                {
                    TOut disposable = creator(inp);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return creator;
        }

        private Func<TIn, int, TOut> WrapSelect(bool isDisposable, Func<TIn, int, TOut> creator)
        {
            if (isDisposable)
                return (TIn inp, int index) =>
                {
                    TOut disposable = creator(inp, index);
                    this.ExecutionContext.AddDisposable(disposable as IDisposable);
                    return disposable;
                };
            else
                return creator;
        }

        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
}
