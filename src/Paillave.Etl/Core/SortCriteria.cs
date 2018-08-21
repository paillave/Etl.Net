using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core
{
    public class SortCriteria<S> : ISortCriteria<S>
    {
        public SortCriteria(Expression<Func<S, IComparable>> field, SortOrder sortOrder = SortOrder.Ascending)
        {
            this.Field = field;
            this.SortOrder = sortOrder;
        }
        public SortOrder SortOrder { get; private set; }
        public Expression<Func<S, IComparable>> Field { get; private set; }
    }
    public static class SortCriteria
    {
        public static SortCriteria<S> Create<S>(Expression<Func<S, IComparable>> field, SortOrder sortOrder = SortOrder.Ascending)
        {
            return new SortCriteria<S>(field, sortOrder);
        }
        public static SortCriteria<S> Create<S>(S prototye, Expression<Func<S, IComparable>> field, SortOrder sortOrder = SortOrder.Ascending)
        {
            return new SortCriteria<S>(field, sortOrder);
        }
    }
    public enum SortOrder { Ascending, Descending }
}
