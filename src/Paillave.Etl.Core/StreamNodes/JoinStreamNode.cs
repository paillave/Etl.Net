using Paillave.Etl.Core.System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
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
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TInLeft, TInRight>(i.Input, i.Input2, i.Exception)));
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
        public static NodeOutputError<TOut, TInLeft, TInRight> LeftJoinKeepErrors<TInLeft, TInRight, TOut>(this ISortedStream<TInLeft> leftStream, string name, IKeyedStream<TInRight> rightStream, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            var ret = new JoinStreamNode<TInLeft, TInRight, TOut>(leftStream, name, null, new JoinArgs<TInLeft, TInRight, TOut>
            {
                RightInputStream = rightStream,
                ResultSelector = resultSelector,
                RedirectErrorsInsteadOfFail = false
            });
            return new NodeOutputError<TOut, TInLeft, TInRight>(ret.Output, ret.Error);
        }
    }
}
