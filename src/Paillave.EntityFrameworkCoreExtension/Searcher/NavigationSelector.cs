using System;
using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Searcher;

public class NavigationSelector<TEntity, TTarget, TId> : INavigationSelector where TEntity : class where TTarget : class
{
    public NavigationSelector(string name, Expression<Func<TEntity, TTarget>> getTargetExpression, SearchDescriptorBase<TTarget, TId> levelDescriptor)
    {
        this.Name = name;
        this.GetTargetExpression = getTargetExpression;
        this.ObjectDescriptor = levelDescriptor;
    }
    // Expression<Func<TEntity, TTarget>>
    public LambdaExpression GetTargetExpression { get; }
    public string Name { get; set; }
    public ISearchDescriptor ObjectDescriptor { get; }
}
