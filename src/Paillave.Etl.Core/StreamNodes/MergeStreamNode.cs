using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class MergeStreamNode<I> : OutputStreamNodeBase<I>
    {
        public MergeStreamNode(IStream<I> inputStream, IStream<I> inputStream2, string name, IEnumerable<string> parentsName = null) : base(inputStream.Context ?? inputStream2.Context, name, parentsName)
        {
            this.CreateOutputStream(inputStream.Observable.Merge(inputStream2.Observable));
        }
    }
    public static partial class StreamEx
    {
        public static IStream<I> Merge<I>(this IStream<I> stream, string name, IStream<I> inputStream2)
        {
            return new MergeStreamNode<I>(stream, inputStream2, name).Output;
        }
    }
}
