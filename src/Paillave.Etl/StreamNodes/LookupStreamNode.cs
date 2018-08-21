using Paillave.Etl.Core;
using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Operators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paillave.Etl.StreamNodes
{
    public class LookupArgs<TInLeft, TInRight, TOut, TKey>
    {
        public IStream<TInRight> RightInputStream { get; set; }
        public Func<TInLeft, TKey> GetLeftStreamKey { get; set; }
        public Func<TInRight, TKey> GetRightStreamKey { get; set; }
        public Func<TInLeft, TInRight, TOut> ResultSelector { get; set; }
        public bool RedirectErrorsInsteadOfFail { get; set; }
    }
    public class LookupStreamNode<TInLeft, TInRight, TOut, TKey> : StreamNodeBase<IStream<TInLeft>, TInLeft, LookupArgs<TInLeft, TInRight, TOut, TKey>>, IStreamNodeOutput<TOut>, IStreamNodeError<ErrorRow<TInLeft, TInRight>>
    {
        public LookupStreamNode(IStream<TInLeft> input, string name, LookupArgs<TInLeft, TInRight, TOut, TKey> arguments) : base(input, name, arguments)
        {
            var rightDicoS = this.Arguments.RightInputStream.Observable.ToList().Map(l => l.ToDictionary(this.Arguments.GetRightStreamKey));
            var matchingS = input.Observable.CombineWithLatest(rightDicoS, (l, rl) => new { Left = l, Right = this.HandleMatching(l, rl) }, true);
            if (arguments.RedirectErrorsInsteadOfFail)
            {
                var errorManagedResult = matchingS.Map(i => base.ErrorManagementWrapFunction<TInLeft, TInRight, TOut>(this.Arguments.ResultSelector)(i.Left, i.Right));
                this.Output = base.CreateStream(nameof(this.Output), errorManagedResult.Filter(i => !i.OnException).Map(i => i.Output));
                this.Error = base.CreateStream(nameof(this.Error), errorManagedResult.Filter(i => i.OnException).Map(i => new ErrorRow<TInLeft, TInRight>(i)));
            }
            else
                this.Output = base.CreateStream(nameof(this.Output), matchingS.Map(i => this.Arguments.ResultSelector(i.Left, i.Right)));
        }

        private TInRight HandleMatching(TInLeft l, Dictionary<TKey, TInRight> rl)
        {
            TInRight r = default(TInRight);
            rl.TryGetValue(this.Arguments.GetLeftStreamKey(l), out r);
            return r;
        }

        public IStream<TOut> Output { get; }
        public IStream<ErrorRow<TInLeft, TInRight>> Error { get; }
    }
}
