using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core
{
    public static class LambdaExpressionEx
    {
        public static PropertyInfo GetPropertyInfo(this LambdaExpression memberLambda)
        {
            var memberSelectorExpression = memberLambda.Body as MemberExpression;
            if (memberSelectorExpression != null)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null) return property;
            }
            throw new ArgumentException("Not a navigation expression", nameof(memberLambda));
        }
        public static List<PropertyInfo> GetPropertyInfos<T>(this Expression<Func<T, object>> expr)
        {
            var vis = new MapperVisitor();
            vis.Visit(expr);
            return vis.PropertyInfos;
        }
        private class MapperVisitor : ExpressionVisitor
        {
            public List<PropertyInfo> PropertyInfos { get; private set; }
            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                NewInstanceVisitor vis = new NewInstanceVisitor();
                vis.Visit(node.Body);
                this.PropertyInfos = vis.MappingSetters;
                return null;
            }
        }
        private class NewInstanceVisitor : ExpressionVisitor
        {
            public List<PropertyInfo> MappingSetters { get; private set; } = new List<PropertyInfo>();
            protected override Expression VisitMember(MemberExpression node)
            {
                this.MappingSetters.Add(node.Member as PropertyInfo);
                return base.VisitMember(node);
            }
        }
    }
}