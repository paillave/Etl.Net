using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace FundProcess.Pms.DataAccess
{
    public static class ModelBuilderEx
    {
        public static IEnumerable<TypeInfo> GetConstructibleTypes(this Assembly assembly)
            => assembly.GetLoadableDefinedTypes().Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition);

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
        public static ModelBuilder ApplyConfigurationsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly, Func<Type, bool> predicate = null)
        {
            var applyEntityConfigurationMethod = typeof(ModelBuilder)
                .GetMethods()
                .Single(
                    e => e.Name == nameof(ModelBuilder.ApplyConfiguration)
                        && e.ContainsGenericParameters
                        && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
            var applyQueryConfigurationMethod = typeof(ModelBuilder)
                .GetMethods()
                .Single(
                    e => e.Name == nameof(ModelBuilder.ApplyConfiguration)
                        && e.ContainsGenericParameters
                        && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition() == typeof(IQueryTypeConfiguration<>));
            foreach (var type in assembly.GetConstructibleTypes())
            {
                // Only accept types that contain a parameterless constructor, are not abstract and satisfy a predicate if it was used.
                if (type.GetConstructor(Type.EmptyTypes) == null || (!predicate?.Invoke(type) ?? false))
                {
                    continue;
                }

                foreach (var @interface in type.GetInterfaces())
                {
                    if (!@interface.IsGenericType)
                    {
                        continue;
                    }

                    if (@interface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        var target = applyEntityConfigurationMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                        target.Invoke(modelBuilder, new[] { Activator.CreateInstance(type) });
                    }
                    else if (@interface.GetGenericTypeDefinition() == typeof(IQueryTypeConfiguration<>))
                    {
                        var target = applyQueryConfigurationMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                        target.Invoke(modelBuilder, new[] { Activator.CreateInstance(type) });
                    }
                }
            }

            return modelBuilder;
        }
        public static ModelBuilder ApplyConfigurationsFromAssembly(this ModelBuilder modelBuilder, Assembly assembly, params object[] parameters)
        {
            var applyEntityConfigurationMethod = typeof(ModelBuilder)
                .GetMethods()
                .Single(
                    e => e.Name == nameof(ModelBuilder.ApplyConfiguration)
                        && e.ContainsGenericParameters
                        && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));
            var applyQueryConfigurationMethod = typeof(ModelBuilder)
                .GetMethods()
                .Single(
                    e => e.Name == nameof(ModelBuilder.ApplyConfiguration)
                        && e.ContainsGenericParameters
                        && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition() == typeof(IQueryTypeConfiguration<>));
            foreach (var type in assembly.GetConstructibleTypes())
            {
                // // Only accept types that contain a parameterless constructor, are not abstract and satisfy a predicate if it was used.
                // if (type.GetConstructor(Type.EmptyTypes) == null || (!predicate?.Invoke(type) ?? false))
                // {
                //     continue;
                // }
                // we suppose there is only one constructor here

                foreach (var @interface in type.GetInterfaces())
                {
                    if (!@interface.IsGenericType)
                    {
                        continue;
                    }

                    if (@interface.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>))
                    {
                        var target = applyEntityConfigurationMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                        target.Invoke(modelBuilder, new[] { Activator.CreateInstance(type, GetSelectedParameters(type, parameters)) });
                    }
                    else if (@interface.GetGenericTypeDefinition() == typeof(IQueryTypeConfiguration<>))
                    {
                        var target = applyQueryConfigurationMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                        target.Invoke(modelBuilder, new[] { Activator.CreateInstance(type, GetSelectedParameters(type, parameters)) });
                    }
                }
            }

            return modelBuilder;
        }
        private static object[] GetSelectedParameters(Type type, object[] parameters)
        {
            return type.GetConstructors()[0].GetParameters()
                .GroupJoin(
                    parameters,
                    i => i.ParameterType,
                    i => i?.GetType(),
                    (l, rs) => new
                    {
                        ParameterInfo = l,
                        Value = rs.FirstOrDefault(),
                        Type = rs.GetType()
                    })
                .Select(i => i.Value)
                .ToArray();
        }
    }
}