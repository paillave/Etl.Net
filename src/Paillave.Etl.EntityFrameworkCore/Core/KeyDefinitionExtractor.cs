using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.EntityFrameworkCore.Core
{
    public class KeyDefinitionExtractor
    {
        public static List<PropertyInfo> GetKeys<T>(Expression<Func<T, object>> getKey)
        {
            var vis = new KeyVisitor();
            vis.Visit(getKey);
            return vis.PropertyInfos;
        }
        public static List<PropertyInfo> GetKeys<T, TKey>(Expression<Func<T, TKey>> getKey)
        {
            var vis = new KeyVisitor();
            vis.Visit(getKey);
            return vis.PropertyInfos;
        }
        private class KeyVisitor : ExpressionVisitor
        {
            public List<PropertyInfo> PropertyInfos { get; } = new List<PropertyInfo>();
            protected override Expression VisitMember(MemberExpression node)
            {
                PropertyInfos.Add((PropertyInfo)node.Member);
                return base.VisitMember(node);
            }
        }
        public class ExpressionKeysResult
        {
            public List<MemberExpression> MemberExpressions { get; } = new List<MemberExpression>();
            public ParameterExpression ParameterExpression { get; set; }
        }
        public static ExpressionKeysResult GetExpressionKeys<T>(Expression<Func<T, object>> getKey)
        {
            var vis = new ExpressionKeyVisitor();
            vis.Visit(getKey);
            return vis.Result;
        }
        public static ExpressionKeysResult GetExpressionKeys<T, TKey>(Expression<Func<T, TKey>> getKey)
        {
            var vis = new ExpressionKeyVisitor();
            vis.Visit(getKey);
            return vis.Result;
        }
        private class ExpressionKeyVisitor : ExpressionVisitor
        {
            public ExpressionKeysResult Result { get; } = new ExpressionKeysResult();
            protected override Expression VisitParameter(ParameterExpression node)
            {
                Result.ParameterExpression = node;
                return node;
            }
            protected override Expression VisitMember(MemberExpression node)
            {
                Result.MemberExpressions.Add(node);
                return node;
            }
        }
    }
}