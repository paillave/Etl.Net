using System.Collections.Generic;
using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Searcher;

public interface ISearchDescriptor
{
    string Name { get; }
    SearchMetadata GetMetadata();
    IDictionary<string, INavigationSelector> Navigations { get; }
    IDictionary<string, IFieldSelector> Fields { get; }
    IFieldSelector? DefaultValueProperty { get; }
    Expression GetFilterExpression(Dictionary<string, List<string>> filters);
}
