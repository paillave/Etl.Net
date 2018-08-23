using Paillave.Etl.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.NodeOutputsOld;
using Paillave.Etl.Core.StreamNodesOld;

namespace Paillave.Etl.StreamNodes
{
    public class JoinArgs<TInLeft, TInRight, TOut>
    {
        public IKeyedStream<TInRight> RightInputStream { get; set; }
        public Func<TInLeft, TInRight, TOut> ResultSelector { get; set; }
        public bool RedirectErrorsInsteadOfFail { get; set; }
    }
    public class JoinStreamNode<TInLeft, TInRight, TOut> : StreamNodeBase<ISortedStream<TInLeft>, TInLeft, JoinArgs<TInLeft, TInRight, TOut>>, IStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TInLeft, TInRight>>
    {
        public JoinStreamNode(ISortedStream<TInLeft> input, string name, JoinArgs<TInLeft, TInRight, TOut> arguments) : base(input, name, arguments)
        {
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = input.Observable.LeftJoin(arguments.RightInputStream.Observable, new SortCriteriaComparer<TInLeft, TInRight>(input.SortCriterias.ToList(), arguments.RightInputStream.SortCriterias.ToList()), base.ErrorManagementWrapFunction(arguments.ResultSelector));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Map(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TInLeft, TInRight>(i)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), input.Observable.LeftJoin(arguments.RightInputStream.Observable, new SortCriteriaComparer<TInLeft, TInRight>(input.SortCriterias.ToList(), arguments.RightInputStream.SortCriterias.ToList()), arguments.ResultSelector));
        }

        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TInLeft, TInRight>> Error { get; }
    }
}
