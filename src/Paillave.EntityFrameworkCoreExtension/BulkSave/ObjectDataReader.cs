using FastMember;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave;

public class ObjectDataReader : IDataReader
{
    private class ValueAccessor(Type type)
    {
        private readonly HashSet<string> _relatedProperties = new(type.GetProperties().Select(i => i.Name));
        private readonly TypeAccessor _accessor = TypeAccessor.Create(type);

        public object? this[object target, string key]
        {
            get
            {
                if (_relatedProperties.Contains(key))
                    return _accessor[target, key];
                else
                    return null;
            }
        }
    }
    private IEnumerator? _source;
    private readonly Dictionary<Type, ValueAccessor> _accessorsByType;
    private readonly HashSet<string> _shadowProperties;
    private readonly Dictionary<string, ValueConverter> _convertibleProperties;
    private readonly DbContext _context;
    private readonly string[] _memberNames;
    private readonly Type[] _effectiveTypes;
    private readonly bool[] _allowNull;
    private readonly string? _tempColumnNumOrderName;
    private int _rowCounter = 0;
    public ObjectDataReader(IEnumerable source, ObjectDataReaderConfig config)
    {
        if (source == null) throw new ArgumentOutOfRangeException("source");

        _accessorsByType = config.Types.ToDictionary(i => i.ClrType, i => new ValueAccessor(i.ClrType));
        _tempColumnNumOrderName = config.TempColumnNumOrderName;
        var sp = config.EfProperties.Where(i => i.IsShadowProperty());
        // var tmp=
        _shadowProperties = new HashSet<string>(sp.Select(i => i.Name));
        _convertibleProperties = config.EfProperties
            .Select(i => new { ValueConverter = i.GetValueConverter(), i.Name })
            .Where(i => i.ValueConverter != null)
            .ToDictionary(i => i.Name, i => i.ValueConverter!);
        _context = config.Context;

        _currentType = null;
        _current = null;
        _memberNames = config.EfProperties.Select(i => i.Name).ToArray();
        if (!string.IsNullOrWhiteSpace(_tempColumnNumOrderName))
            _memberNames = _memberNames.Union(new[] { _tempColumnNumOrderName }).ToArray();
        _effectiveTypes = config.EfProperties.Select(i => Nullable.GetUnderlyingType(i.ClrType) ?? i.ClrType).ToArray();
        _allowNull = config.EfProperties.Select(i => i.IsColumnNullable()).ToArray();
        _source = source.GetEnumerator();
    }
    private object? _current;
    protected object Current => _current ?? throw new InvalidOperationException("No current item");
    private Type? _currentType;


    public int Depth => 0;

    public DataTable GetSchemaTable()
    {
        throw new NotImplementedException();
    }
    public void Close()
    {
        HasRows = false;
        _current = null;
        _currentType = null;
        _source = null;
    }

    public bool HasRows { get; private set; } = true;

    public bool NextResult()
    {
        HasRows = false;
        return false;
    }
    public bool Read()
    {
        if (HasRows)
        {
            var tmp = _source;
            if (tmp != null && tmp.MoveNext())
            {
                _current = tmp.Current;
                _currentType = Current.GetType();
                _rowCounter++;
                return true;
            }
            else
            {
                HasRows = false;
            }
        }
        _currentType = null;
        _current = null;
        return false;
    }

    public int RecordsAffected => 0;

    public void Dispose()
    {
        HasRows = false;
        _current = null;
        _currentType = null;
        _source = null;
    }

    public int FieldCount => _memberNames.Length;

    public bool IsClosed => _source == null;

    public bool GetBoolean(int i) => (bool)(this[i] ?? throw new InvalidOperationException("null value instead of boolean"));

    public byte GetByte(int i) => (byte)(this[i] ?? throw new InvalidOperationException("null value instead of byte"));

    public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferOffset, int length)
    {
        byte[] s = (byte[])(this[i] ?? throw new InvalidOperationException("null value instead of byte[]"));
        int available = s.Length - (int)fieldOffset;
        if (available <= 0) return 0;

        int count = Math.Min(length, available);
        buffer ??= new byte[count];
        Buffer.BlockCopy(s, (int)fieldOffset, buffer, bufferOffset, count);
        return count;
    }

    public char GetChar(int i) => (char)(this[i] ?? throw new InvalidOperationException("null value instead of char"));

    public long GetChars(int i, long fieldOffset, char[]? buffer, int bufferOffset, int length)
    {
        string s = (string)(this[i] ?? throw new InvalidOperationException("null value instead of string"));
        int available = s.Length - (int)fieldOffset;
        if (available <= 0) return 0;

        int count = Math.Min(length, available);
        buffer ??= new char[count];
        s.CopyTo((int)fieldOffset, buffer, bufferOffset, count);
        return count;
    }
    public string GetDataTypeName(int i) => (_effectiveTypes == null ? typeof(object) : _effectiveTypes[i]).Name;
    public DateTime GetDateTime(int i) => (DateTime)(this[i] ?? throw new InvalidOperationException("null value instead of DateTime"));
    public decimal GetDecimal(int i) => (decimal)(this[i] ?? throw new InvalidOperationException("null value instead of decimal"));
    public double GetDouble(int i) => (double)(this[i] ?? throw new InvalidOperationException("null value instead of double"));
    public Type GetFieldType(int i) => _effectiveTypes == null ? typeof(object) : _effectiveTypes[i];
    public float GetFloat(int i) => (float)(this[i] ?? throw new InvalidOperationException("null value instead of float"));
    public Guid GetGuid(int i) => (Guid)(this[i] ?? throw new InvalidOperationException("null value instead of Guid"));
    public short GetInt16(int i) => (short)(this[i] ?? throw new InvalidOperationException("null value instead of short"));
    public int GetInt32(int i) => (int)(this[i] ?? throw new InvalidOperationException("null value instead of int"));
    public long GetInt64(int i) => (long)(this[i] ?? throw new InvalidOperationException("null value instead of long"));
    public string GetName(int i) => _memberNames[i];
    public int GetOrdinal(string name) => Array.IndexOf(_memberNames, name);
    public string GetString(int i) => (string)(this[i] ?? throw new InvalidOperationException("null value instead of string"));
    public object GetValue(int i) => this[i] ?? throw new InvalidOperationException("null value instead of object");
    public int GetValues(object[] values)
    {
        int count = Math.Min(values.Length, this._memberNames.Length);
        for (int i = 0; i < count; i++) values[i] = this.GetValue(this.GetOrdinal(this._memberNames[i]));
        return count;
    }
    public bool IsDBNull(int i) => this[i] is DBNull;
    public IDataReader GetData(int i)
    {
        throw new NotImplementedException();
    }
    private readonly Dictionary<string, Dictionary<string, bool>> _shadowOfEntityDico = new();
    public object this[string name]
    {
        get
        {
            if (name == _tempColumnNumOrderName)
            {
                return _rowCounter - 1;
            }
            else if (_shadowProperties.Contains(name))
            {
                var etr = _context.Entry(Current);
                if (_shadowOfEntityDico.TryGetValue(etr.Metadata.Name, out var subDic))
                {
                    if (subDic.TryGetValue(name, out var hasIt))
                    {
                        if (hasIt)
                            return etr.Property(name).CurrentValue ?? Constants.DBNull;
                        else
                            return Constants.DBNull;
                    }
                    var elt = etr.Properties.FirstOrDefault(i => i.Metadata.Name == name);
                    subDic[name] = elt != null;
                    if (elt != null)
                        return elt.CurrentValue ?? Constants.DBNull;
                    else
                        return Constants.DBNull;
                }
                var elt2 = etr.Properties.FirstOrDefault(i => i.Metadata.Name == name);
                _shadowOfEntityDico[etr.Metadata.Name] = new Dictionary<string, bool> { [name] = elt2 != null };
                if (elt2 == null) return Constants.DBNull;
                return elt2.CurrentValue ?? Constants.DBNull;
                // if(etr.Properties.FirstOrDefault())
                // return etr.Property(name).CurrentValue;
            }
            else
            {
                if (_currentType == null) throw new Exception("No current item");
                if (!_accessorsByType.TryGetValue(_currentType, out var acc)) return Constants.DBNull;
                var val = acc[Current, name];
                if (val == null) return Constants.DBNull;
                if (_convertibleProperties.TryGetValue(name, out var converter)) return converter.ConvertToProvider(val) ?? Constants.DBNull;
                return val;
            }
        }
    }
    public object this[int i] => this[_memberNames[i]];
}
public class ObjectDataReaderConfig
{
    public required IEnumerable<IProperty> EfProperties { get; set; }
    public required IEnumerable<IEntityType> Types { get; set; }
    public required DbContext Context { get; set; }
    public string? TempColumnNumOrderName { get; set; }
}
