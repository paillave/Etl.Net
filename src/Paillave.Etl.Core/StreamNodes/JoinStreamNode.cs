using Paillave.Etl.Core.System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class JoinStreamNode<TInLeft, TInRight, O> : OutputErrorStreamNodeBase<O, ErrorRow<TInLeft, TInRight>>
    {
        public JoinStreamNode(ISortedStream<TInLeft> leftInputStream, IKeyedStream<TInRight> rightInputStream, Func<TInLeft, TInRight, O> resultSelector, string name, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentsName = null) : base(leftInputStream.Context ?? rightInputStream.Context, name, parentsName)
        {
            base.CreateOutputStream(leftInputStream.Observable.LeftJoin(rightInputStream.Observable, new SortCriteriaComparer<TInLeft, TInRight>(leftInputStream.SortCriterias.ToList(), rightInputStream.SortCriterias.ToList()), resultSelector));
        }
    }
    public static partial class StreamEx
    {
        public static IStream<O> LeftJoin<I1, I2, O>(this ISortedStream<I1> leftStream, string name, IKeyedStream<I2> rightStream, Func<I1, I2, O> resultSelector)
        {
            return new JoinStreamNode<I1, I2, O>(leftStream, rightStream, resultSelector, name, false).Output;
        }
        public static NodeOutput<O, I1, I2> LeftJoinKeepErrors<I1, I2, O>(this ISortedStream<I1> leftStream, string name, IKeyedStream<I2> rightStream, Func<I1, I2, O> resultSelector)
        {
            var ret = new JoinStreamNode<I1, I2, O>(leftStream, rightStream, resultSelector, name, false);
            return new NodeOutput<O, I1, I2>(ret.Output, ret.Error);
        }
    }
    public class ErrorRow<TInLeft, TInRight>
    {
        public ErrorRow(TInLeft leftInput, TInRight rightInput, Exception exception)
        {
            this.LeftInput = leftInput;
            this.RightInput = rightInput;
            this.Exception = exception;
        }

        public TInLeft LeftInput { get; }
        public TInRight RightInput { get; }
        public Exception Exception { get; }
    }
    public class NodeOutput<S, I1, I2>
    {
        public NodeOutput(IStream<S> output, IStream<ErrorRow<I1, I2>> error)
        {
            this.Output = output;
            this.Error = error;
        }
        public IStream<S> Output { get; }
        public IStream<ErrorRow<I1, I2>> Error { get; }
    }
}
