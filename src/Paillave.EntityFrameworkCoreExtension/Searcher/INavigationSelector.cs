using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Searcher;

public interface INavigationSelector
{
    string Name { get; }
    ISearchDescriptor ObjectDescriptor { get; }
    LambdaExpression GetTargetExpression { get; }
}
