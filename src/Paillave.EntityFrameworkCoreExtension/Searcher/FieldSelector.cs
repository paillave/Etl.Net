using System;
using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Searcher;
public class FieldSelector<TEntity, TValue> : IFieldSelector where TEntity : class
{
    public FieldSelector(string name, Expression<Func<TEntity, TValue>> getValue, Func<string, TValue> convertFromString)
    {
        this.Name = name;
        this.GetValueExpression = getValue;
        this.ConvertFromString = i => convertFromString(i);
    }
    public string Name { get; }
    // Expression<Func<TEntity, TValue>>
    public LambdaExpression GetValueExpression { get; }
    public Func<string, object?> ConvertFromString { get; }
}
