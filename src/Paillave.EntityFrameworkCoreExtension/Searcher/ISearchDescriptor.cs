using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Paillave.EntityFrameworkCoreExtension.Searcher
{
    public interface ISearchDescriptor
    {
        string Name { get; }
        SearchMetadata GetMetadata();
        IDictionary<string, INavigationSelector> Navigations { get; }
        IDictionary<string, IFieldSelector> Fields { get; }
        IFieldSelector DefaultValueProperty { get; }
        Expression GetFilterExpression(Dictionary<string, List<string>> filters);
        // Expression GetValueExpression(string pathToProperty);
        // List<List<int>> SearchIds(IDictionary<string, IEnumerable<string>> filters, string pathToGroupProperty);
        // List<int> SearchIds(IDictionary<string, IEnumerable<string>> filters);
    }
}