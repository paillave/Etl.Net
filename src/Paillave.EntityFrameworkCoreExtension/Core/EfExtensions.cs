using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Paillave.EntityFrameworkCoreExtension.Core
{
    public static class EfExtensions
    {
        #region Remove from ef core version 5
        // see here: https://blog.oneunicorn.com/2020/01/12/toquerystring/ and https://github.com/dotnet/efcore/issues/6482
        public static string ToQueryString<TEntity>(this IQueryable<TEntity> query) where TEntity : class
        {
            var enumerator = query.Provider.Execute<IEnumerable<TEntity>>(query.Expression).GetEnumerator();
            var relationalCommandCache = enumerator.Private("_relationalCommandCache");
            var selectExpression = relationalCommandCache.Private<SelectExpression>("_selectExpression");
            var factory = relationalCommandCache.Private<IQuerySqlGeneratorFactory>("_querySqlGeneratorFactory");

            var sqlGenerator = factory.Create();
            var command = sqlGenerator.GetCommand(selectExpression);

            string sql = command.CommandText;
            return sql;
        }
        private static object Private(this object obj, string privateField) => obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        private static T Private<T>(this object obj, string privateField) => (T)obj?.GetType().GetField(privateField, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
        #endregion

        public static EntityEntry<TEntity> EntryWithoutDetectChanges<TEntity>(this DbContext context, TEntity entity)
                    where TEntity : class
        {
            var entryWithoutDetectChangesMethodInfo = context.GetType().GetMethod("EntryWithoutDetectChanges", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(TEntity) }, null);
            return (EntityEntry<TEntity>)entryWithoutDetectChangesMethodInfo.Invoke(context, new object[] { entity });
        }
        private static Regex regex = new Regex(@"SELECT\s+(?<ref>[[]?.+?[]]?)[.].+?\sFROM", RegexOptions.Singleline & RegexOptions.IgnoreCase);
        public static Task DeleteWhereAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> filter) where T : class
            => DeleteWhereAsync(dbSet, filter, CancellationToken.None);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "Reviewed")]
        public static async Task DeleteWhereAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> filter, CancellationToken cancellationToken) where T : class
        {
            // await dbSet.Where(filter).ExecuteDeleteAsync();
        }

        public static Expression<Func<T1, TResult>> ApplyPartialRight<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, Expression expressionValue)
        {
            var parameterToBeReplaced = expression.Parameters[1];
            var visitor = new ReplacementVisitor(parameterToBeReplaced, expressionValue);
            var newBody = visitor.Visit(expression.Body);
            return Expression.Lambda<Func<T1, TResult>>(newBody, expression.Parameters[0]);
        }

        public static Expression<Func<T1, TResult>> ApplyPartialRight<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, T2 value)
        {
            var parameterToBeReplaced = expression.Parameters[1];
            var constant = Expression.Constant(value, parameterToBeReplaced.Type);
            return ApplyPartialRight(expression, constant);
        }
        public static Expression<Func<T2, TResult>> ApplyPartialLeft<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, Expression expressionValue)
        {
            var parameterToBeReplaced = expression.Parameters[0];
            var visitor = new ReplacementVisitor(parameterToBeReplaced, expressionValue);
            var newBody = visitor.Visit(expression.Body);
            return Expression.Lambda<Func<T2, TResult>>(newBody, expression.Parameters[1]);
        }
        public static Expression<Func<T2, TResult>> ApplyPartialLeft<T1, T2, TResult>(this Expression<Func<T1, T2, TResult>> expression, T1 value)
        {
            var parameterToBeReplaced = expression.Parameters[0];
            var constant = Expression.Constant(value, parameterToBeReplaced.Type);
            return ApplyPartialLeft(expression, constant);
        }
        public static Expression<Func<TResult>> ApplyPartial<T1, TResult>(this Expression<Func<T1, TResult>> expression, Expression expressionValue)
        {
            var parameterToBeReplaced = expression.Parameters[0];
            var visitor = new ReplacementVisitor(parameterToBeReplaced, expressionValue);
            var newBody = visitor.Visit(expression.Body);
            return Expression.Lambda<Func<TResult>>(newBody, expression.Parameters[1]);
        }
        public static Expression<Func<TResult>> ApplyPartial<T1, TResult>(this Expression<Func<T1, TResult>> expression, T1 value)
        {
            var parameterToBeReplaced = expression.Parameters[0];
            var constant = Expression.Constant(value, parameterToBeReplaced.Type);
            return ApplyPartial(expression, constant);
        }
    }
}