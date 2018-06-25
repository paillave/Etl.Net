using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class CombineLatestStreamNode<I, I2, O> : OutputErrorStreamNodeBase<O, ErrorRow<I>>
    {
        public CombineLatestStreamNode(Stream<I> inputStream, Stream<I2> inputStream2, Func<I, I2, O> resultSelector, string name, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentsName = null)
            : base(inputStream, name, parentsName)
        {
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream.Observable.CombineLatest(inputStream2.Observable, base.ErrorManagementWrapFunction(resultSelector));
                this.CreateOutputStream(errorManagedResult.Where(i => !i.OnException).Select(i => i.Output));
                this.CreateErrorStream(errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<I>(i.Input, i.Exception)));
            }
            else
                this.CreateOutputStream(inputStream.Observable.CombineLatest(inputStream2.Observable, resultSelector));
        }
    }
    public static partial class StreamEx
    {
        public static Stream<O> CombineLatest<I, I2, O>(this Stream<I> stream, string name, Stream<I2> inputStream2, Func<I, I2, O> resultSelector, bool redirectErrorsInsteadOfFail = false)
        {
            return new CombineLatestStreamNode<I, I2, O>(stream, inputStream2, resultSelector, name, redirectErrorsInsteadOfFail).Output;
        }
    }
}
