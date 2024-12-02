using System;
using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Searcher;
public interface IFieldSelector
{
    string Name { get; }
    LambdaExpression GetValueExpression { get; }
    Func<string, object?> ConvertFromString { get; }
}
