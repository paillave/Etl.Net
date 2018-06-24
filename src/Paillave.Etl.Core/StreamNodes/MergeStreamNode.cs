using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class MergeStreamNode<I> : TransformStreamNodeBase<I, I>
    {
        public MergeStreamNode(Stream<I> inputStream, Stream<I> inputStream2, string name, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            this.Output = base.CreateOutputStream(inputStream.Observable.Merge(inputStream2.Observable));
        }
    }
    public static partial class StreamEx
    {
        public static Stream<I> Merge<I>(this Stream<I> stream, string name, Stream<I> inputStream2)
        {
            return new MergeStreamNode<I>(stream, inputStream2, name).Output;
        }
    }
}
