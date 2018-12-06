using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.Debugger.Coordinator
{
    public class Inspector
    {
        public List<EltDescription> GetEtlList(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            return GetEtlList(assembly, assemblyPath);
        }
        public List<EltDescription> GetEtlList(Assembly assembly, string assemblyPath = null) => assembly.DefinedTypes
                .SelectMany(i => i.DeclaredMethods)
                .Select(methodInfo =>
                {
                    if (!methodInfo.IsStatic || methodInfo.ReturnType != typeof(void)) return null;
                    var parameters = methodInfo.GetParameters();
                    if (parameters.Length != 1) return null;
                    var parameterType = parameters[0].ParameterType;
                    if (parameterType.GenericTypeArguments.Length != 1) return null;
                    var configType = parameterType.GenericTypeArguments[0];
                    var genericType = typeof(ISingleStream<>).MakeGenericType(configType);
                    if (parameterType != genericType) return null;
                    return new EltDescription
                    {
                        Summary = new EltDescriptionSummary
                        {
                            AssemblyFilePath = assemblyPath,
                            StreamTransformationName = methodInfo.Name,
                            ClassName = methodInfo.DeclaringType.Name,
                            Namespace = methodInfo.DeclaringType.Namespace,
                        },
                        ClassType = methodInfo.DeclaringType,
                        StreamConfigType = configType,
                        StreamType = parameterType,
                        StreamTransformationMethodInfo = methodInfo,
                    };
                })
                .Where(i => i != null)
                .ToList();
    }
}