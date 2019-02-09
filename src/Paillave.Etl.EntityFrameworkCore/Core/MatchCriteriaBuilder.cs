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
        private Expression<Func<TInLeft, TEntity, bool>> _matchExpression;
        private ParameterExpression _leftParamExpression;
        private ParameterExpression _entityParamExpression;
        public MatchCriteriaBuilder(Expression<Func<TInLeft, TKey>> leftKeyExpression, Expression<Func<TEntity, TKey>> rightKeyExpression, Expression<Func<TEntity, bool>> defaultDatasetCriteria = null)
        {
            // http://www.albahari.com/nutshell/predicatebuilder.aspx
            var leftKeyProperties = KeyDefinitionExtractor.GetKeys(leftKeyExpression);
            var rightKeyProperties = KeyDefinitionExtractor.GetKeys(rightKeyExpression);
            ParameterExpression leftParam = Expression.Parameter(typeof(TInLeft), "left");
            ParameterExpression rightParam = Expression.Parameter(typeof(TEntity), "entity");
            var equalityExpressions = leftKeyProperties.Zip(rightKeyProperties, (l, r) =>
              {
                  var leftValExpression = Expression.Property(leftParam, l);
                  var rightValExpression = Expression.Property(rightParam, r);
                  return Expression.Equal(leftValExpression, rightValExpression);
              });
            BinaryExpression expr = null;
            foreach (var equalityExpression in equalityExpressions)
            {
                if (expr == null) expr = equalityExpression;
                else expr = Expression.AndAlso(expr, equalityExpression);
            }
            if (defaultDatasetCriteria != null)
            {
                expr = Expression.AndAlso(expr, Expression.Invoke(defaultDatasetCriteria, rightParam));
            }
            _leftParamExpression = leftParam;
            _entityParamExpression = rightParam;
            _matchExpression = Expression.Lambda<Func<TInLeft, TEntity, bool>>(expr, _leftParamExpression, _entityParamExpression);
        }
        public Expression<Func<TEntity, bool>> GetCriteriaExpression(TInLeft value)
        {
            var parameterToBeReplaced = _leftParamExpression;
            var constantExpression = Expression.Constant(value, parameterToBeReplaced.Type);
            var visitor = new ReplacementVisitor(parameterToBeReplaced, constantExpression);
            var newBody = visitor.Visit(_matchExpression.Body);
            return Expression.Lambda<Func<TEntity, bool>>(newBody, _entityParamExpression);
        }
    }
}