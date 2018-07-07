using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reactive.Linq;

namespace Paillave.Etl.Core.StreamNodes
{
    public static partial class StreamEx
    {
        public static IStream<TOut> CrossApply<TIn, TOut, TDataSource>(this IStream<TIn> stream, string name) where TDataSource : DataSourceNodeBase<TIn, TOut>, new()
        {
            TDataSource ds = new TDataSource();
            ds.Initialize(stream.ExecutionContext, name);
            ds.SetupStream(stream.Observable);
            return ds.Output;
        }
    }
}
