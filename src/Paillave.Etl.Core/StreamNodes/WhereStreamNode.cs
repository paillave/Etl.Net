using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class WhereStreamNode<I> : OutputErrorStreamNodeBase<I, ErrorRow<I>>
    {
        public WhereStreamNode(Stream<I> inputStream, Func<I, bool> predicate, string name, bool redirectErrorsInsteadOfFail, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            if (redirectErrorsInsteadOfFail)
            {
                var errorManagedResult = inputStream.Observable.Select(base.ErrorManagementWrapFunction(predicate));
                this.CreateOutputStream(errorManagedResult.Where(i => !i.OnException).Where(i => i.Output).Select(i => i.Input));
                this.CreateErrorStream(errorManagedResult.Where(i => i.OnException).Select(i => new ErrorRow<I>(i.Input, i.Exception)));
            }
            else
                this.CreateOutputStream(inputStream.Observable.Where(predicate));
        }
    }
    public static partial class StreamEx
    {
        public static Stream<I> Where<I>(this Stream<I> stream, string name, Func<I, bool> predicate, bool redirectErrorsInsteadOfFail = false)
        {
            return new WhereStreamNode<I>(stream, predicate, name, redirectErrorsInsteadOfFail).Output;
        }
    }
}
