using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Paillave.Etl.Core.System
{
    public class SortCriteria<S>
    {
        public SortCriteria(Expression<Func<S, IComparable>> field, SortOrder sortOrder = SortOrder.Ascending)
        {
            this.Field = field;
            this.SortOrder = sortOrder;
        }
        public SortOrder SortOrder { get; private set; }
        public Expression<Func<S, IComparable>> Field { get; private set; }
    }
    public enum SortOrder { Ascending, Descending }
}
