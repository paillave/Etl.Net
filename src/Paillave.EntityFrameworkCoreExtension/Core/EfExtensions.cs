using System;
using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Core;

public static class EfExtensions
{
    /// <summary>
    /// Applies a partial function application by replacing the second parameter of the given expression with a specified expression value.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the original expression.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the original expression.</typeparam>
    /// <typeparam name="TResult">The return type of the original expression.</typeparam>
    /// <param name="expression">The original expression with two parameters.</param>
    /// <param name="expressionValue">The expression value to replace the second parameter of the original expression.</param>
    /// <returns>A new expression with the second parameter replaced by the specified expression value.</returns>
    public static Expression<Func<T1, TResult>> ApplyPartialRight<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, Expression expressionValue)
    {
        var parameterToBeReplaced = expression.Parameters[1];
        var visitor = new ReplacementVisitor(parameterToBeReplaced, expressionValue);
        var newBody = visitor.Visit(expression.Body) ?? throw new InvalidOperationException("The expression could not be applied");
        return Expression.Lambda<Func<T1, TResult>>(newBody, expression.Parameters[0]);
    }

    /// <summary>
    /// Partially applies a value to the second parameter of a given expression.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the expression.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the expression.</typeparam>
    /// <typeparam name="TResult">The type of the result of the expression.</typeparam>
    /// <param name="expression">The expression to which the value will be partially applied.</param>
    /// <param name="value">The value to be applied to the second parameter of the expression.</param>
    /// <returns>An expression with the second parameter replaced by the specified value.</returns>
    public static Expression<Func<T1, TResult>> ApplyPartialRight<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, T2 value)
    {
        var parameterToBeReplaced = expression.Parameters[1];
        var constant = Expression.Constant(value, parameterToBeReplaced.Type);
        return ApplyPartialRight(expression, constant);
    }
    /// <summary>
    /// Applies a partial function by replacing the first parameter of the given expression with the specified expression value.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the original expression.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the original expression and the first parameter of the resulting expression.</typeparam>
    /// <typeparam name="TResult">The return type of the expression.</typeparam>
    /// <param name="expression">The original expression with two parameters.</param>
    /// <param name="expressionValue">The expression value to replace the first parameter of the original expression.</param>
    /// <returns>A new expression with the first parameter replaced by the specified expression value.</returns>
    public static Expression<Func<T2, TResult>> ApplyPartialLeft<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, Expression expressionValue)
    {
        var parameterToBeReplaced = expression.Parameters[0];
        var visitor = new ReplacementVisitor(parameterToBeReplaced, expressionValue);
        var newBody = visitor.Visit(expression.Body) ?? throw new InvalidOperationException("The expression could not be applied");
        return Expression.Lambda<Func<T2, TResult>>(newBody, expression.Parameters[1]);
    }
    /// <summary>
    /// Partially applies the first parameter of a given expression with a specified value.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter of the expression.</typeparam>
    /// <typeparam name="T2">The type of the second parameter of the expression.</typeparam>
    /// <typeparam name="TResult">The type of the result of the expression.</typeparam>
    /// <param name="expression">The expression to partially apply.</param>
    /// <param name="value">The value to apply to the first parameter of the expression.</param>
    /// <returns>An expression with the first parameter replaced by the specified value.</returns>
    public static Expression<Func<T2, TResult>> ApplyPartialLeft<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, T1 value)
    {
        var parameterToBeReplaced = expression.Parameters[0];
        var constant = Expression.Constant(value, parameterToBeReplaced.Type);
        return ApplyPartialLeft(expression, constant);
    }
    /// <summary>
    /// Applies a partial evaluation to the given expression by replacing the first parameter with the specified expression value.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter in the original expression.</typeparam>
    /// <typeparam name="TResult">The return type of the expression.</typeparam>
    /// <param name="expression">The original expression to be partially evaluated.</param>
    /// <param name="expressionValue">The expression value to replace the first parameter in the original expression.</param>
    /// <returns>A new expression with the first parameter replaced by the specified expression value.</returns>
    public static Expression<Func<TResult>> ApplyPartial<T1, TResult>(this Expression<Func<T1, TResult>> expression, Expression expressionValue)
    {
        var parameterToBeReplaced = expression.Parameters[0];
        var visitor = new ReplacementVisitor(parameterToBeReplaced, expressionValue);
        var newBody = visitor.Visit(expression.Body) ?? throw new InvalidOperationException("The expression could not be applied");
        return Expression.Lambda<Func<TResult>>(newBody, expression.Parameters[1]);
    }
    /// <summary>
    /// Applies a partial evaluation to the given expression by replacing the first parameter with a constant value.
    /// </summary>
    /// <typeparam name="T1">The type of the parameter to be replaced.</typeparam>
    /// <typeparam name="TResult">The return type of the expression.</typeparam>
    /// <param name="expression">The expression to be partially evaluated.</param>
    /// <param name="value">The constant value to replace the first parameter in the expression.</param>
    /// <returns>A new expression with the first parameter replaced by the given constant value.</returns>
    public static Expression<Func<TResult>> ApplyPartial<T1, TResult>(this Expression<Func<T1, TResult>> expression, T1 value)
    {
        var parameterToBeReplaced = expression.Parameters[0];
        var constant = Expression.Constant(value, parameterToBeReplaced.Type);
        return ApplyPartial(expression, constant);
    }
}
