using Paillave.Etl.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Core.NodeOutputs;
using Paillave.Etl.Core.StreamNodes;

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
        public JoinStreamNode(ISortedStream<TInLeft> input, string name, IEnumerable<string> parentNodeNamePath, JoinArgs<TInLeft, TInRight, TOut> arguments) : base(input, name, parentNodeNamePath, arguments)
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
    public static partial class StreamEx
    {
        public static IStream<TOut> LeftJoin<TInLeft, TInRight, TOut>(this ISortedStream<TInLeft> leftStream, string name, IKeyedStream<TInRight> rightStream, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            return new JoinStreamNode<TInLeft, TInRight, TOut>(leftStream, name, null, new JoinArgs<TInLeft, TInRight, TOut>
            {
                RightInputStream = rightStream,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = false
            }).Output;
        }
        public static INodeOutputError<TOut, TInLeft, TInRight> LeftJoinKeepErrors<TInLeft, TInRight, TOut>(this ISortedStream<TInLeft> leftStream, string name, IKeyedStream<TInRight> rightStream, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            var ret = new JoinStreamNode<TInLeft, TInRight, TOut>(leftStream, name, null, new JoinArgs<TInLeft, TInRight, TOut>
            {
                RightInputStream = rightStream,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = true
            });
            return new NodeOutputError<JoinStreamNode<TInLeft, TInRight, TOut>, TOut, TInLeft, TInRight>(ret);
        }
    }
}
