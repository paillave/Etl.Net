using System;
using System.Collections.Generic;
using Paillave.Etl.GraphApi.Provider.Writers;

namespace Paillave.Etl.GraphApi.Provider;

public class ODataExpressionConverterSettings
{
    public static ODataExpressionConverterSettings Default
    {
        get; set;
    } = new ODataExpressionConverterSettings();

    private readonly List<IMethodCallWriter> _methodWriters = new List<IMethodCallWriter>();
    private readonly List<IMemberCallWriter> _memberWriters = new List<IMemberCallWriter>();
    private readonly List<IValueWriter> _valueWriters = new List<IValueWriter>();

    public IEnumerable<IMethodCallWriter> MethodCallWriters
    {
        get
        {
            return _methodWriters;
        }
    }
    public IEnumerable<IValueWriter> ValueWriters
    {
        get
        {
            return _valueWriters;
        }
    }

    public IEnumerable<IMemberCallWriter> MemberCallWriters
    {
        get
        {
            return _memberWriters;
        }
    }

    public ISerializationBinder SerializationBinder { get; set; } = DefaultSerializationBinder.Instance;

    public void RegisterValueWriter<T>(Func<T, string> function)
    {
        RegisterValueWriter(new GenericValueWriter(typeof(T), v => function((T)v)));
    }

    public void RegisterValueWriter(Type t, Func<object, string> function)
    {
        RegisterValueWriter(new GenericValueWriter(t, function));
    }

    public void RegisterValueWriter(IValueWriter valueWriter)
    {
        _valueWriters.Add(valueWriter);
    }

    public ODataExpressionConverterSettings()
    {
        DateWriterModule.RegisterWriters(this);
        StringWriterModule.RegisterWriters(this);
        MathWriterModule.RegisterWriters(this);
        EnumWriterModule.RegisterWriters(this);
        GuidWriterModule.RegisterWriters(this);

        this.RegisterValueWriter<bool>(v => v ? "true" : "false");
    }

    public void RegisterMember<TType>(string method)
    {
        RegisterMember(typeof(TType), method);
    }

    public void RegisterMember<TType>(string method, string function)
    {
        RegisterMember(typeof(TType), method, function);
    }
    public void RegisterMember(Type type, string method)
    {
        RegisterMember(type, method, method.ToLower());
    }
    public void RegisterMember(Type type, string method, string function)
    {
        RegisterMember(new GenericMemberWriter(type, method, function));
    }

    public void RegisterMember(IMemberCallWriter writer)
    {
        _memberWriters.Add(writer);
    }


    public void RegisterMethod<TType>(string method)
    {
        RegisterMethod(typeof(TType), method);
    }

    public void RegisterMethod(Type type, string method)
    {
        RegisterMethod(type, method, method.ToLower());
    }

    public void RegisterMethod<TType>(string method, string function)
    {
        RegisterMethod(typeof(TType), method, function);
    }
    public void RegisterMethod<TType>(string method, Func<string, IEnumerable<string>, string> function)
    {
        RegisterMethod(typeof(TType), method, function);
    }
    public void RegisterMethod(Type type, string method, string function)
    {
        RegisterMethod(new GenericMethodWriter(type, method, function));
    }
    public void RegisterMethod(Type type, string method, Func<string, IEnumerable<string>, string> function)
    {
        RegisterMethod(new GenericMethodWriter(type, method, function));
    }

    public void RegisterMethod(IMethodCallWriter writer)
    {
        _methodWriters.Add(writer);
    }

}
