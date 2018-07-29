using System;
using System.Linq.Expressions;

namespace Paillave.Etl.Core.System
{
    public interface ISortCriteria<S>
    {
        Expression<Func<S, IComparable>> Field { get; }
        SortOrder SortOrder { get; }
    }
}