using System;
using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Searcher;
public class FieldSelector<TEntity, TValue>(string name, Expression<Func<TEntity, TValue>> getValue, Func<string, TValue> convertFromString) : IFieldSelector where TEntity : class
{
    public string Name { get; } = name;
    // Expression<Func<TEntity, TValue>>
    public LambdaExpression GetValueExpression { get; } = getValue;
    public Func<string, object?> ConvertFromString { get; } = i => convertFromString(i);
}
