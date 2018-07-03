using Paillave.Etl.Core.System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class JoinStreamNode<TInLeft, TInRight, TOut> : StreamNodeBase, IStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TInLeft, TInRight>>
    {
        public JoinStreamNode(ISortedStream<TInLeft> leftInputStream, string name, IKeyedStream<TInRight> rightInputStream, Func<TInLeft, TInRight, TOut> resultSelector, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentNodeNamePath = null)
        {
            this.Initialize(leftInputStream.ExecutionContext ?? rightInputStream.ExecutionContext, name, parentNodeNamePath);
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = leftInputStream.Observable.LeftJoin(rightInputStream.Observable, new SortCriteriaComparer<TInLeft, TInRight>(leftInputStream.SortCriterias.ToList(), rightInputStream.SortCriterias.ToList()), base.ErrorManagementWrapFunction(resultSelector));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<TInLeft, TInRight>(i.Input, i.Input2, i.Exception)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), leftInputStream.Observable.LeftJoin(rightInputStream.Observable, new SortCriteriaComparer<TInLeft, TInRight>(leftInputStream.SortCriterias.ToList(), rightInputStream.SortCriterias.ToList()), resultSelector));
        }
        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TInLeft, TInRight>> Error { get; }
    }
    public static partial class StreamEx
    {
        public static IStream<TOut> LeftJoin<TInLeft, TInRight, TOut>(this ISortedStream<TInLeft> leftStream, string name, IKeyedStream<TInRight> rightStream, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            return new JoinStreamNode<TInLeft, TInRight, TOut>(leftStream, name, rightStream, resultSelector, false).Output;
        }
        public static NodeOutputError<TOut, TInLeft, TInRight> LeftJoinKeepErrors<TInLeft, TInRight, TOut>(this ISortedStream<TInLeft> leftStream, string name, IKeyedStream<TInRight> rightStream, Func<TInLeft, TInRight, TOut> resultSelector)
        {
            var ret = new JoinStreamNode<TInLeft, TInRight, TOut>(leftStream, name, rightStream, resultSelector, false);
            return new NodeOutputError<TOut, TInLeft, TInRight>(ret.Output, ret.Error);
        }
    }
    //public class ErrorRow<TInLeft, TInRight>
    //{
    //    public ErrorRow(TInLeft leftInput, TInRight rightInput, Exception exception)
    //    {
    //        this.LeftInput = leftInput;
    //        this.RightInput = rightInput;
    //        this.Exception = exception;
    //    }

    //    public TInLeft LeftInput { get; }
    //    public TInRight RightInput { get; }
    //    public Exception Exception { get; }
    //}
    //public class NodeOutput<S, I1, I2>
    //{
    //    public NodeOutput(IStream<S> output, IStream<ErrorRow<I1, I2>> error)
    //    {
    //        this.Output = output;
    //        this.Error = error;
    //    }
    //    public IStream<S> Output { get; }
    //    public IStream<ErrorRow<I1, I2>> Error { get; }
    //}
}
