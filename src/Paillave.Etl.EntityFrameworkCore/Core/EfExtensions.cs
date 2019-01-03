using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.EntityFrameworkCore.Core
{
    public static class EfExtensions
    {
        public static void DeleteWhere<T>(this DbContext db, Expression<Func<T, bool>> filter) where T : class
        {
            string selectSql = db.Set<T>().Where(filter).ToString();
            string fromWhere = selectSql.Substring(selectSql.IndexOf("FROM"));
            string deleteSql = "DELETE [Extent1] " + fromWhere;
            db.Database.ExecuteSqlCommand(deleteSql);
        }
        public static Expression<Func<T1, TResult>> ApplyPartialRight<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, T2 value)
        {
            var parameterToBeReplaced = expression.Parameters[1];
            var constant = Expression.Constant(value, parameterToBeReplaced.Type);
            var visitor = new ReplacementVisitor(parameterToBeReplaced, constant);
            var newBody = visitor.Visit(expression.Body);
            return Expression.Lambda<Func<T1, TResult>>(newBody, expression.Parameters[0]);
        }
        public static Expression<Func<T2, TResult>> ApplyPartialLeft<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, T1 value)
        {
            var parameterToBeReplaced = expression.Parameters[0];
            var constant = Expression.Constant(value, parameterToBeReplaced.Type);
            var visitor = new ReplacementVisitor(parameterToBeReplaced, constant);
            var newBody = visitor.Visit(expression.Body);
            return Expression.Lambda<Func<T2, TResult>>(newBody, expression.Parameters[1]);
        }
        private class ReplacementVisitor : ExpressionVisitor
        {
            private readonly Expression original, replacement;

            public ReplacementVisitor(Expression original, Expression replacement)
            {
                this.original = original;
                this.replacement = replacement;
            }

            public override Expression Visit(Expression node)
            {
                return node == original ? replacement : base.Visit(node);
            }
        }
    }
}