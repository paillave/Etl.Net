using System;

namespace Paillave.Etl.Core
{
    public static partial class SubProcessEx
    {
        public static IStream<TOut> SubProcess<TIn, TOut>(this IStream<TIn> stream, string name, Func<ISingleStream<TIn>, IStream<TOut>> subProcess, bool noParallelisation = false)
        {
            return new SubProcessStreamNode<TIn, TOut>(name, new SubProcessArgs<TIn, TOut>
            {
                NoParallelisation = noParallelisation,
                SubProcess = subProcess,
                Stream = stream
            }).Output;
        }
    }
}
