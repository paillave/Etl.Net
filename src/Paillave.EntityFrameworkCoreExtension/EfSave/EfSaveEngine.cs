using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Paillave.EntityFrameworkCoreExtension.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Paillave.EntityFrameworkCoreExtension.EfSave
{
    public class EfSaveEngine<T> where T : class
    {
        private Expression<Func<T, T, bool>> _findConditionExpression;
        private List<PropertyInfo> _keyPropertyInfos;
        private DbContext _context;
        private readonly CancellationToken _cancellationToken;
        public EfSaveEngine(DbContext context, CancellationToken cancellationToken, params Expression<Func<T, object>>[] pivotKeys)
        {
            this._cancellationToken = cancellationToken;
            _context = context;
            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
                throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);
            _keyPropertyInfos = entityType.GetProperties().Where(i => !i.IsShadowProperty() && i.IsPrimaryKey()).Select(i => i.PropertyInfo).ToList();
            List<List<PropertyInfo>> propertyInfosForPivot;
            if ((pivotKeys?.Length ?? 0) == 0)
                propertyInfosForPivot = new List<List<PropertyInfo>> { entityType.FindPrimaryKey().Properties.Select(i => i.PropertyInfo).ToList() };
            else
                propertyInfosForPivot = pivotKeys.Select(pivotKey => KeyDefinitionExtractor.GetKeys(pivotKey)).ToList();

            _findConditionExpression = CreateFindConditionExpression(propertyInfosForPivot);
        }
        private Expression<Func<T, T, bool>> CreateFindConditionExpression(List<List<PropertyInfo>> propertyInfosForPivotSet)
        {
            ParameterExpression leftParam = Expression.Parameter(typeof(T), "i");
            ParameterExpression rightParam = Expression.Parameter(typeof(T), "rightParam");
            Expression predicateBody = null;
            foreach (var propertyInfosForPivot in propertyInfosForPivotSet)
            {
                var pivotPartExpression = CreatePivotPartExpression(propertyInfosForPivot, leftParam, rightParam);
                if (predicateBody == null)
                    predicateBody = pivotPartExpression;
                else
                    predicateBody = Expression.OrElse(predicateBody, pivotPartExpression);
            }
            return Expression.Lambda<Func<T, T, bool>>(predicateBody, new[] { leftParam, rightParam });
        }
        private Expression CreatePivotPartExpression(List<PropertyInfo> propertyInfosForPivot, ParameterExpression leftParam, ParameterExpression rightParam)
        {
            Expression predicatePivotPart = null;
            foreach (var propertyInfoForPivot in propertyInfosForPivot)
            {
                var equalityExpression = CreateEqualityExpression(propertyInfoForPivot, leftParam, rightParam);
                if (predicatePivotPart == null)
                    predicatePivotPart = equalityExpression;
                else
                    predicatePivotPart = Expression.AndAlso(predicatePivotPart, equalityExpression);
            }
            return predicatePivotPart;
        }
        private Expression CreateEqualityExpression(PropertyInfo propertyInfo, ParameterExpression leftParam, ParameterExpression rightParam)
        {
            Expression leftValue = Expression.Property(leftParam, propertyInfo);
            Expression rightValue = Expression.Property(rightParam, propertyInfo);
            return Expression.Equal(leftValue, rightValue);
        }
        public async Task SaveAsync(IList<T> entities, bool doNotUpdateIfExists = false, bool insertOnly = false)
        {
            var contextSet = _context.Set<T>();
            foreach (var entity in entities)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                if (insertOnly)
                    contextSet.Add(entity);
                else
                    InsertOrUpdateEntity(doNotUpdateIfExists, contextSet, entity);
            }
            await _context.SaveChangesAsync(_cancellationToken);
        }

        private void InsertOrUpdateEntity(bool doNotUpdateIfExists, DbSet<T> contextSet, T entity)
        {
            var expr3 = _findConditionExpression.ApplyPartialLeft(entity);
            T existingEntity = contextSet.AsNoTracking().FirstOrDefault(expr3);
            if (existingEntity != null)
            {
                if (!doNotUpdateIfExists)
                {
                    foreach (var keyPropertyInfo in _keyPropertyInfos)
                    {
                        object val = keyPropertyInfo.GetValue(existingEntity);
                        keyPropertyInfo.SetValue(entity, val);
                    }
                    // contextSet.Update(entity);
                }
            }
            // else
            //     contextSet.Add(entity);
            contextSet.Update(entity);
            if (existingEntity == null)
            {
                _context.Entry(entity).State = EntityState.Added;
            }
        }
    }
}
