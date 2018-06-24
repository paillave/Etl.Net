using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public class CombineLatestStreamNode<I, I2, O> : OutputStreamNodeBase<O>
    {
        public CombineLatestStreamNode(Stream<I> inputStream, Stream<I2> inputStream2, Func<I, I2, O> resultSelector, string name, IEnumerable<string> parentsName = null)
            : base(inputStream, name, parentsName)
        {
            this.CreateOutputStream(inputStream.Observable.CombineLatest(inputStream2.Observable, resultSelector));
        }
    }
    public static partial class StreamEx
    {
        public static Stream<O> CombineLatest<I, I2, O>(this Stream<I> stream, string name, Stream<I2> inputStream2, Func<I, I2, O> resultSelector)
        {
            return new CombineLatestStreamNode<I, I2, O>(stream, inputStream2, resultSelector, name).Output;
        }
    }
}
