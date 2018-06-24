using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public abstract class ExpressionTransformStreamNodeBase<I, O> : StreamNodeBase
    {
        public ExpressionTransformStreamNodeBase(Stream<I> inputStream, string name, IEnumerable<string> parentNodeNamePath = null) : base(inputStream.Context, name, parentNodeNamePath)
        {
        }
        protected Stream<T> CreateStream<T>(string streamName, IObservable<T> observable)
        {
            return new Stream<T>(base.ExecutionContext, base.NodeNamePath, streamName, observable);
        }
        public Stream<O> Output { get; protected set; }
    }
}
