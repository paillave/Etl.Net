using System;
using System.Reflection;

namespace Paillave.Etl.GraphApi.Provider;

/// <summary>
/// The default serialization binder used when resolving and loading classes from type names.
/// </summary>
public class DefaultSerializationBinder : ISerializationBinder
{
    internal static readonly DefaultSerializationBinder Instance = new DefaultSerializationBinder();

    private readonly ThreadSafeStore<TypeNameKey, Type> _typeCache = new ThreadSafeStore<TypeNameKey, Type>(GetTypeFromTypeNameKey);

    private static Type GetTypeFromTypeNameKey(TypeNameKey typeNameKey)
    {
        string assemblyName = typeNameKey.AssemblyName;
        string typeName = typeNameKey.TypeName;

        if (assemblyName != null)
        {
            Assembly assembly = Assembly.Load(assemblyName);

            Type type = assembly?.GetType(typeName);

            return type;
        }
        else
        {
            return Type.GetType(typeName);
        }
    }

    internal struct TypeNameKey : IEquatable<TypeNameKey>
    {
        internal readonly string AssemblyName;
        internal readonly string TypeName;

        public TypeNameKey(string assemblyName, string typeName)
        {
            AssemblyName = assemblyName;
            TypeName = typeName;
        }

        public override int GetHashCode()
        {
            return ((AssemblyName != null) ? AssemblyName.GetHashCode() : 0)
                ^ ((TypeName != null) ? TypeName.GetHashCode() : 0);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeNameKey))
                return false;

            return Equals((TypeNameKey)obj);
        }

        public bool Equals(TypeNameKey other)
        {
            return (AssemblyName == other.AssemblyName && TypeName == other.TypeName);
        }
    }

    /// <summary>
    /// When overridden in a derived class, controls the binding of a serialized object to a type.
    /// </summary>
    /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object.</param>
    /// <param name="typeName">Specifies the <see cref="T:System.Type"/> name of the serialized object.</param>
    /// <returns>
    /// The type of the object the formatter creates a new instance of.
    /// </returns>
    public Type BindToType(string assemblyName, string typeName)
    {
        return _typeCache.Get(new TypeNameKey(assemblyName, typeName));
    }

    /// <summary>
    /// When overridden in a derived class, controls the binding of a serialized object to a type.
    /// </summary>
    /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
    /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object. </param>
    /// <param name="typeName">Specifies the <see cref="T:System.Type"/> name of the serialized object. </param>
    public void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
        assemblyName = serializedType.Assembly.FullName;
        typeName = serializedType.FullName;
    }
}
