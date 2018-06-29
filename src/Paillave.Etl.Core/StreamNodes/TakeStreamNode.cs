using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class TakeStreamNode<I> : OutputStreamNodeBase<I>
    {
        public TakeStreamNode(IStream<I> inputStream, int count, string name, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            this.CreateOutputStream(inputStream.Observable.Take(count));
        }
    }
    public static partial class StreamEx
    {
        public static IStream<I> Take<I>(this IStream<I> stream, string name, int count)
        {
            return new TakeStreamNode<I>(stream, count, name).Output;
        }
    }
}
