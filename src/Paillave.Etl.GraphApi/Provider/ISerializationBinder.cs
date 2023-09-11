using System;

namespace Paillave.Etl.GraphApi.Provider;
public interface ISerializationBinder
{
    void BindToName(Type serializedType, out string assemblyName, out string typeName);
    Type BindToType(string assemblyName, string typeName);
}
