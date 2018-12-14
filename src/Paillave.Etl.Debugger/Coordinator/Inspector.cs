using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Paillave.Etl.Core.Streams;

namespace Paillave.Etl.Debugger.Coordinator
{
    public class Inspector
    {
        public Inspector(string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);
            this.Processes = GetEtlList(assembly, assemblyPath);
        }
        public List<ProcessDescription> Processes { get; }
        public JobDefinitionStructure GetJobDefinitionStructure(string className, string @namespace, string streamTransformationName)
        {
            var process = Processes
                            .First(i =>
                                i.Summary.ClassName == className &&
                                i.Summary.Namespace == @namespace &&
                                i.Summary.StreamTransformationName == streamTransformationName);


            var action = process.StreamTransformationMethodInfo.CreateDelegate(typeof(Action<>).MakeGenericType(typeof(ISingleStream<>).MakeGenericType(process.StreamConfigType)));

            var processRunner = Activator.CreateInstance(typeof(StreamProcessRunner<>).MakeGenericType(process.StreamConfigType), action, process.Summary.StreamTransformationName);

            return processRunner.GetType().GetMethod(nameof(StreamProcessRunner<int>.GetDefinitionStructure)).Invoke(processRunner, new object[] { }) as JobDefinitionStructure;
        }
        public static List<ProcessDescription> GetEtlList(Assembly assembly, string assemblyPath = null) => assembly.DefinedTypes
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
                    return new ProcessDescription
                    {
                        Summary = new ProcessDescriptionSummary
                        {
                            AssemblyFilePath = assemblyPath,
                            StreamTransformationName = methodInfo.Name,
                            ClassName = methodInfo.DeclaringType.Name,
                            Namespace = methodInfo.DeclaringType.Namespace,
                            Parameters = configType.GetProperties().Select(i => i.Name).ToList()
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