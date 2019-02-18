using System;
using System.Linq;
using System.Linq.Expressions;

namespace Paillave.Etl.EntityFrameworkCore.Core
{
    public class MatchCriteriaBuilder
    {
        public static MatchCriteriaBuilder<TInLeft, TEntity, TKey> Create<TInLeft, TEntity, TKey>(Expression<Func<TInLeft, TKey>> leftKeyExpression, Expression<Func<TEntity, TKey>> rightKeyExpression, Expression<Func<TEntity, bool>> defaultDatasetCriteria = null)
            => new MatchCriteriaBuilder<TInLeft, TEntity, TKey>(leftKeyExpression, rightKeyExpression, defaultDatasetCriteria);
    }
    public class MatchCriteriaBuilder<TInLeft, TEntity, TKey>
    {
        private readonly Expression<Func<TEntity, bool>> _defaultDatasetCriteria;
        private readonly KeyDefinitionExtractor.ExpressionKeysResult _inputKeyExpressionStructure;
        private readonly KeyDefinitionExtractor.ExpressionKeysResult _entityKeyExpressionStructure;
        public MatchCriteriaBuilder(Expression<Func<TInLeft, TKey>> leftKeyExpression, Expression<Func<TEntity, TKey>> entityKeyExpression, Expression<Func<TEntity, bool>> defaultDatasetCriteria = null)
        {
            this._defaultDatasetCriteria = defaultDatasetCriteria;
            this._inputKeyExpressionStructure = KeyDefinitionExtractor.GetExpressionKeys(leftKeyExpression);
            this._entityKeyExpressionStructure = KeyDefinitionExtractor.GetExpressionKeys(entityKeyExpression);
        }
        public Expression<Func<TEntity, bool>> GetCriteriaExpression(TInLeft value)
        {
            var paramExpr = Expression.Constant(value);

            var equalityExpressions = _inputKeyExpressionStructure.MemberExpressions
                .Zip(_entityKeyExpressionStructure.MemberExpressions, (i, e) => Expression.Equal(i, e));

            BinaryExpression expr = null;
            foreach (var equalityExpression in equalityExpressions)
            {
                if (expr == null) expr = equalityExpression;
                else expr = Expression.AndAlso(expr, equalityExpression);
            }
            if (_defaultDatasetCriteria != null)
            {
                expr = Expression.AndAlso(expr, Expression.Invoke(_defaultDatasetCriteria, _entityKeyExpressionStructure.ParameterExpression));
            }

            var fExpr = Expression.Lambda<Func<TInLeft, TEntity, bool>>(expr, _inputKeyExpressionStructure.ParameterExpression, _entityKeyExpressionStructure.ParameterExpression);

            return fExpr.ApplyPartialLeft(value);
        }
    }
}