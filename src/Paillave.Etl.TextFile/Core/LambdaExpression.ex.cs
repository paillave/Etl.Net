using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.TextFile.Core
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
    }
}