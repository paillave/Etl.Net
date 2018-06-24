using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class TakeStreamNode<I> : TransformStreamNodeBase<I, I>
    {
        public TakeStreamNode(Stream<I> inputStream, int count, string name, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            this.Output = base.CreateOutputStream(inputStream.Observable.Take(count));
        }
    }
    public static partial class StreamEx
    {
        public static Stream<I> Take<I>(this Stream<I> stream, string name, int count)
        {
            return new TakeStreamNode<I>(stream, count, name).Output;
        }
    }
}
