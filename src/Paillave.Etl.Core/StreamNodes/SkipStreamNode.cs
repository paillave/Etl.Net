using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class SkipStreamNode<I> : TransformStreamNodeBase<I, I>
    {
        public SkipStreamNode(Stream<I> inputStream, int count, string name, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            this.Output = base.CreateStream(nameof(Output), inputStream.Observable.Skip(count));
        }
    }
    public static partial class StreamEx
    {
        public static Stream<I> Skip<I>(this Stream<I> stream, string name, int count)
        {
            return new SkipStreamNode<I>(stream, count, name).Output;
        }
    }
}
