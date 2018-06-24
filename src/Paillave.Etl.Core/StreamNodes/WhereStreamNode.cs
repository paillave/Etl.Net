using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class WhereStreamNode<I> : TransformStreamNodeBase<I, I>
    {
        public WhereStreamNode(Stream<I> inputStream, Func<I, bool> predicate, string name, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            this.Output = base.CreateOutputStream(inputStream.Observable.Where(predicate));
        }
    }
    public static partial class StreamEx
    {
        public static Stream<I> Where<I>(this Stream<I> stream, string name, Func<I, bool> predicate)
        {
            return new WhereStreamNode<I>(stream, predicate, name).Output;
        }
    }
}
