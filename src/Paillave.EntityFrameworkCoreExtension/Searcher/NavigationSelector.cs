using System;
using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Searcher
{
    public class NavigationSelector<TEntity, TTarget, TId>(string name,
        Expression<Func<TEntity, TTarget>> getTargetExpression, 
        SearchDescriptorBase<TTarget, TId> levelDescriptor) : INavigationSelector where TEntity : class where TTarget : class
    {
        // Expression<Func<TEntity, TTarget>>
        public LambdaExpression GetTargetExpression { get; } = getTargetExpression;
        public string Name { get; set; } = name;
        public ISearchDescriptor ObjectDescriptor { get; } = levelDescriptor;
    }
}