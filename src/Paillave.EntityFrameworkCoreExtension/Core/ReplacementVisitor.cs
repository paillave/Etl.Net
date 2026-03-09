using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Core;

public class ReplacementVisitor(Expression original, Expression replacement) : ExpressionVisitor
{
    private readonly Expression original = original, replacement = replacement;

    public override Expression? Visit(Expression? node)
    {
        return node == original ? replacement : base.Visit(node);
    }
}