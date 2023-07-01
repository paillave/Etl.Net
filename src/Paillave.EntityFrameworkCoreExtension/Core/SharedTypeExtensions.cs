using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Paillave.EntityFrameworkCoreExtension.Core
{
    internal static class SharedTypeExtensions
    {
        public static IEnumerable<TypeInfo> GetConstructibleTypes(this Assembly assembly)
               => assembly.GetLoadableDefinedTypes().Where(
                   t => !t.IsAbstract
                       && !t.IsGenericTypeDefinition);

        public static IEnumerable<TypeInfo> GetLoadableDefinedTypes(this Assembly assembly)
        {
            try
            {
                return assembly.DefinedTypes;
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).Select(IntrospectionExtensions.GetTypeInfo);
            }
        }
    }
}
