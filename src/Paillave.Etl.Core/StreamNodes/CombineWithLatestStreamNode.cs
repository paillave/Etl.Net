using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class CombineWithLatestStreamNode<I, I2, O> : TransformStreamNodeBase<I, O>
    {
        public CombineWithLatestStreamNode(Stream<I> inputStream, Stream<I2> inputStream2, Func<I, I2, O> resultSelector, string name, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            this.Output = base.CreateStream(nameof(Output), inputStream.Observable.CombineLatest(inputStream2.Observable, resultSelector));
        }
    }
    public static partial class StreamEx
    {
        public static Stream<O> CombineWithLatest<I, I2, O>(this Stream<I> stream, string name, Stream<I2> inputStream2, Func<I, I2, O> resultSelector)
        {
            return new CombineWithLatestStreamNode<I, I2, O>(stream, inputStream2, resultSelector, name).Output;
        }
    }
}
