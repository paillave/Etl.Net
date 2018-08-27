using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class JoinArgs<TInLeft, TInRight, TOut>
    {
        public ISortedStream<TInLeft> LeftInputStream { get; set; }
        public IKeyedStream<TInRight> RightInputStream { get; set; }
        public Func<TInLeft, TInRight, TOut> ResultSelector { get; set; }
        public bool RedirectErrorsInsteadOfFail { get; set; }
    }
    public class JoinStreamNode<TInLeft, TInRight, TOut> : StreamNodeBase<TOut, IStream<TOut>, JoinArgs<TInLeft, TInRight, TOut>>
    {
        public JoinStreamNode(string name, JoinArgs<TInLeft, TInRight, TOut> args) : base(name, args)
        {
        }

        protected override IStream<TOut> CreateOutputStream(JoinArgs<TInLeft, TInRight, TOut> args)
        {
            args.LeftInputStream.Observable.LeftJoin(args.RightInputStream.Observable, new SortCriteriaComparer<TInLeft, TInRight>(args.LeftInputStream.SortCriterias.ToList(), args.RightInputStream.SortCriterias.ToList()), args.ResultSelector);
            throw new NotImplementedException();
        }
    }
}
