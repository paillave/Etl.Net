using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace Paillave.Etl.Core.Helpers.MapperFactories
{
    public class PropertyMapper
        {
            public PropertyMapper(PropertyDescription columnNameMap)
            {
                PropertyInfo = GetPropertyInfo(columnNameMap.MemberLamda);
                TypeConverter = TypeDescriptor.GetConverter(PropertyInfo.PropertyType);
                CultureInfo = columnNameMap.CultureInfo;
            }
            public static PropertyInfo GetPropertyInfo(LambdaExpression memberLamda)
            {
                var memberSelectorExpression = memberLamda.Body as MemberExpression;
                if (memberSelectorExpression != null)
                {
                    var property = memberSelectorExpression.Member as PropertyInfo;
                    if (property != null) return property;
                }
                throw new ArgumentException("Not a navigation expression", nameof(memberLamda));
            }
            public PropertyInfo PropertyInfo { get; }
            public TypeConverter TypeConverter { get; }
            public CultureInfo CultureInfo { get; }
        }
}
