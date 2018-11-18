using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Paillave.Etl.EntityFrameworkCore.Core
{
    public class EfMatcher<TInLeft, TEntity, TCtx>
        where TCtx : DbContext
        where TEntity : class
    {
        private Expression<Func<TInLeft, TEntity, bool>> _match;
        private Queue<TEntity> _cachedEntities;
        private int _cacheSize;
        public EfMatcher(Expression<Func<TInLeft, TEntity, bool>> match, int cacheSize = 1000)
        {
            _match = match;
            _cachedEntities = new Queue<TEntity>();
            _cacheSize = cacheSize;
        }

        public TEntity GetMatch(TCtx ctx, TInLeft input)
        {
            var matchExp = ApplyPartialLeft(_match, input);
            var ret = _cachedEntities.AsQueryable().FirstOrDefault(matchExp);
            if (ret != null) return ret;
            var dbSet = ctx.Set<TEntity>();
            ret = dbSet.AsNoTracking().FirstOrDefault(matchExp);
            if (_cachedEntities.Count >= _cacheSize) _cachedEntities.Dequeue();
            _cachedEntities.Enqueue(ret);
            return ret;
        }

        private static Expression<Func<T1, TResult>> ApplyPartialRight<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> expression, T2 value)
        {
            var parameterToBeReplaced = expression.Parameters[1];
            var constant = Expression.Constant(value, parameterToBeReplaced.Type);
            var visitor = new ReplacementVisitor(parameterToBeReplaced, constant);
            var newBody = visitor.Visit(expression.Body);
            return Expression.Lambda<Func<T1, TResult>>(newBody, expression.Parameters[0]);
        }
        private static Expression<Func<T2, TResult>> ApplyPartialLeft<T1, T2, TResult>(Expression<Func<T1, T2, TResult>> expression, T1 value)
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