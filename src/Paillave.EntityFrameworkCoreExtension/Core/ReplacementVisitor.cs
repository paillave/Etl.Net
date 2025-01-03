using System.Linq.Expressions;

namespace Paillave.EntityFrameworkCoreExtension.Core
{
    public class ReplacementVisitor : ExpressionVisitor
    {
        private readonly Expression original, replacement;

        public ReplacementVisitor(Expression original, Expression replacement)
        {
            this.original = original;
            this.replacement = replacement;
        }

        public override Expression? Visit(Expression? node)
        {
            return node == original ? replacement : base.Visit(node);
        }
    }
}