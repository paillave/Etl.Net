using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class MapStreamNode<I, O> : TransformStreamNodeBase<I, O>
    {
        public MapStreamNode(Stream<I> inputStream, Func<I, O> mapper, string name, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            this.Output = base.CreateOutputStream(inputStream.Observable.Select(mapper));
        }
        public MapStreamNode(Stream<I> inputStream, Func<I, int, O> mapper, string name, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            this.Output = base.CreateOutputStream(inputStream.Observable.Select(mapper));
        }
    }
    public static partial class StreamEx
    {
        public static Stream<O> Map<I, O>(this Stream<I> stream, string name, Func<I, O> mapper)
        {
            return new MapStreamNode<I, O>(stream, mapper, name).Output;
        }
        public static Stream<O> Map<I, O>(this Stream<I> stream, string name, Func<I, int, O> mapper)
        {
            return new MapStreamNode<I, O>(stream, mapper, name).Output;
        }
    }
}
