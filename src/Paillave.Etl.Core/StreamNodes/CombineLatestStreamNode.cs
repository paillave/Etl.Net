using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class CombineLatestStreamNode<I1, I2, O> : OutputErrorStreamNodeBase<O, ErrorRow<I1>>
    {
        public CombineLatestStreamNode(IStream<I1> inputStream, IStream<I2> inputStream2, Func<I1, I2, O> resultSelector, string name, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentsName = null)
            : base(inputStream, name, parentsName)
        {
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream.Observable.CombineLatest(inputStream2.Observable, base.ErrorManagementWrapFunction(resultSelector));
                this.CreateOutputStream(errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.CreateErrorStream(errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<I1>(i.Input, i.Exception)));
            }
            else
                this.CreateOutputStream(inputStream.Observable.CombineLatest(inputStream2.Observable, resultSelector));
        }
    }
    public static partial class StreamEx
    {
        public static IStream<O> CombineLatest<I, I2, O>(this IStream<I> stream, string name, IStream<I2> inputStream2, Func<I, I2, O> resultSelector)
        {
            return new CombineLatestStreamNode<I, I2, O>(stream, inputStream2, resultSelector, name, false).Output;
        }
        public static NodeOutput<O, I> CombineLatestKeepErrors<I, I2, O>(this IStream<I> stream, string name, IStream<I2> inputStream2, Func<I, I2, O> resultSelector)
        {
            var ret = new CombineLatestStreamNode<I, I2, O>(stream, inputStream2, resultSelector, name, true);
            return new NodeOutput<O, I>(ret.Output, ret.Error);
        }
    }
}
