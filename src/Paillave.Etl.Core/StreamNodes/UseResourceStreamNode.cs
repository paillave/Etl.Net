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
    public class CreateResourceArgs<TIn, TOut> where TOut : IDisposable
    {
        public Func<TIn, TOut> Mapper { get; set; } = null;
        public bool RedirectErrorsInsteadOfFail { get; set; }
    }
    public class UseResourceStreamNode<TIn, TOut> : StreamNodeBase<IStream<TIn>, TIn, CreateResourceArgs<TIn, TOut>>, IStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TIn>> where TOut : IDisposable
    {
        public UseResourceStreamNode(IStream<TIn> input, string name, CreateResourceArgs<TIn, TOut> arguments) : base(input, name, arguments)
        {
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = input.Observable.Map(base.ErrorManagementWrapFunction(WrapCreator(arguments.Mapper)));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Map(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TIn>(i)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), input.Observable.Map(WrapCreator(arguments.Mapper)));
        }

        private Func<TIn, TOut> WrapCreator(Func<TIn, TOut> creator)
        {
            return (TIn inp) =>
            {
                TOut disposable = creator(inp);
                this.ExecutionContext.AddDisposable(disposable);
                return disposable;
            };
        }

        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TIn>> Error { get; }
    }
}
