using Paillave.Etl.Core.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace Paillave.Etl.Core.StreamNodes
{
    public class EnsureSortedNode<I> : SortedOutputStreamNodeBase<I>
    {
        public EnsureSortedNode(IStream<I> inputStream, string name, IEnumerable<SortCriteria<I>> sortCriterias, IEnumerable<string> parentsName = null) : base(inputStream, name, parentsName)
        {
            this.CreateOutputStream(inputStream.Observable, sortCriterias);
        }
    }
    public static partial class StreamEx
    {
        public static ISortedStream<I> EnsureSorted<I>(this IStream<I> stream, string name, params SortCriteria<I>[] sortCriterias)
        {
            return new EnsureSortedNode<I>(stream, name, sortCriterias).Output;
        }
    }
}
