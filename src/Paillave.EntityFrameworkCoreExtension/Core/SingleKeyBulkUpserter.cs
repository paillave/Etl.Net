using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Paillave.EntityFrameworkCoreExtension.Core
{
    public class SingleKeyBulkUpserter<TIn, TCtx, TKey>
        where TIn : class
        where TCtx : DbContext
    {
        private MethodInfo _containsMethodInfo;
        private Func<TIn, TKey> _getKey;
        private Expression<Func<TIn, TKey>> _getKeyExpr;
        public SingleKeyBulkUpserter(Expression<Func<TIn, TKey>> getKeyExpr)
        {
            _containsMethodInfo = GetContainsMethodInfo();
            _getKey = getKeyExpr.Compile();
            _getKeyExpr = getKeyExpr;
        }
        public void ProcessBatch(IEnumerable<TIn> items, TCtx dbContext)
        {
            var existingItemsFromDb = GetExistingElements(dbContext, items.Select(_getKey).ToArray());
            var entityType = dbContext.Model.FindRuntimeEntityType(typeof(TIn));
            if (entityType == null)
                throw new InvalidOperationException($"The entity type {typeof(TIn).FullName} is not part of the model");
            var primaryKey = entityType.FindPrimaryKey();
            var primaryKeyParameters = primaryKey?.Properties.Select(i => i.PropertyInfo).Where(i => i != null).Cast<PropertyInfo>().ToList() ?? new List<PropertyInfo>();
            var itemsReadyToUpdate = items.GroupJoin(existingItemsFromDb, _getKey, _getKey, (l, r) =>
            {
                var matchFromDb = r.FirstOrDefault();
                if (matchFromDb != null)
                    foreach (var pk in primaryKeyParameters)
                        pk.SetValue(l, pk.GetValue(matchFromDb));
                return l;
            });
            dbContext.UpdateRange(itemsReadyToUpdate);
            dbContext.SaveChanges();
        }
        private MethodInfo GetContainsMethodInfo()
        {
            return typeof(System.Linq.Enumerable).GetMethods().Where(i => i.Name == "Contains").First(i =>
            {
                if (!i.ContainsGenericParameters) return false;
                var gArgs = i.GetGenericArguments();
                if (gArgs.Length != 1) return false;
                var pars = i.GetParameters().ToList();
                if (pars.Count != 2) return false;
                if (!(pars[0].ParameterType.IsGenericType && pars[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>))) return false;
                if (!(!pars[1].ParameterType.IsGenericType && pars[1].ParameterType == gArgs[0])) return false;
                return true;
            }).MakeGenericMethod(typeof(TKey));
        }
        private IEnumerable<TIn> GetExistingElements(TCtx dbCtx, TKey[] lst)
        {
            var param = Expression.Parameter(typeof(TIn));
            var tmp = Expression.Lambda<Func<TIn, bool>>(Expression.Call(
                _containsMethodInfo,
                Expression.Constant(lst),
                Expression.Property(param, (PropertyInfo)((MemberExpression)_getKeyExpr.Body).Member)
                ), param);
            return dbCtx.Set<TIn>().AsNoTracking().Where(tmp);
        }
    }
}