using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Paillave.Etl.EntityFrameworkCore.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Paillave.Etl.EntityFrameworkCore.EfSave
{
    public class EfSaveEngine<T> where T : class
    {
        private Expression<Func<T, T, bool>> _findConditionExpression;
        private List<PropertyInfo> _keyPropertyInfos;
        private DbContext _context;
        public EfSaveEngine(DbContext context, Expression<Func<T, object>> pivotKey = null)
        {
            _context = context;
            var entityType = context.Model.FindEntityType(typeof(T));
            if (entityType == null)
                throw new InvalidOperationException("DbContext does not contain EntitySet for Type: " + typeof(T).Name);
            _keyPropertyInfos = entityType.GetProperties().Where(i => !i.IsShadowProperty).Where(i => i.IsPrimaryKey()).Select(i => i.PropertyInfo).ToList();
            List<PropertyInfo> propertyInfosForPivot;
            if (pivotKey == null)
                propertyInfosForPivot = entityType.FindPrimaryKey().Properties.Select(i => i.PropertyInfo).ToList();
            else
                propertyInfosForPivot = KeyDefinitionExtractor.GetKeys(pivotKey);

            _findConditionExpression = CreateFindConditionExpression(propertyInfosForPivot);
        }
        private Expression<Func<T, T, bool>> CreateFindConditionExpression(List<PropertyInfo> propertyInfosForPivot)
        {
            ParameterExpression leftParam = Expression.Parameter(typeof(T), "leftParam");
            ParameterExpression rightParam = Expression.Parameter(typeof(T), "rightParam");
            Expression predicateBody = null;
            foreach (var propertyInfoForPivot in propertyInfosForPivot)
            {
                if (predicateBody == null)
                    predicateBody = CreateEqualityExpression(propertyInfoForPivot, leftParam, rightParam);
                else
                    predicateBody = Expression.AndAlso(predicateBody, CreateEqualityExpression(propertyInfoForPivot, leftParam, rightParam));
            }
            return Expression.Lambda<Func<T, T, bool>>(predicateBody, new[] { leftParam, rightParam });
        }
        private Expression CreateEqualityExpression(PropertyInfo propertyInfo, ParameterExpression leftParam, ParameterExpression rightParam)
        {
            Expression leftValue = Expression.Property(leftParam, propertyInfo);
            Expression rightValue = Expression.Property(rightParam, propertyInfo);
            return Expression.Equal(leftValue, rightValue);
        }
        public void Save(IList<T> entities)
        {
            var contextSet = _context.Set<T>();
            foreach (var entity in entities)
            {
                var expr = _findConditionExpression.ApplyPartialLeft(entity);
                T existingEntity = contextSet.AsNoTracking().FirstOrDefault(expr);
                if (existingEntity != null)
                {
                    foreach (var keyPropertyInfo in _keyPropertyInfos)
                    {
                        object val = keyPropertyInfo.GetValue(existingEntity);
                        keyPropertyInfo.SetValue(entity, val);
                    }
                    contextSet.Update(existingEntity);
                }
                else
                    contextSet.Add(existingEntity);
            }
            _context.SaveChanges();
        }
    }
}
