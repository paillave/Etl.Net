using FastMember;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Paillave.EntityFrameworkCoreExtension.BulkSave
{
    public class ObjectDataReader : IDataReader
    {
        private class ValueAccessor
        {
            private HashSet<string> _relatedProperties;
            private TypeAccessor _accessor;

            public ValueAccessor(Type type)
            {
                _accessor = TypeAccessor.Create(type);
                _relatedProperties = new HashSet<string>(type.GetProperties().Select(i => i.Name));
            }
            public object this[object target, string key]
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
        private IEnumerator _source;
        private readonly Dictionary<Type, ValueAccessor> _accessorsByType;
        private readonly HashSet<string> _shadowProperties;
        private readonly Dictionary<string, ValueConverter> _convertibleProperties;
        private readonly DbContext _context;
        private readonly string[] _memberNames;
        private readonly Type[] _effectiveTypes;
        private readonly bool[] _allowNull;
        private string _tempColumnNumOrderName;
        private int _rowCounter = 0;
        public ObjectDataReader(IEnumerable source, ObjectDataReaderConfig config)
        {
            if (source == null) throw new ArgumentOutOfRangeException("source");

            _accessorsByType = config.Types.ToDictionary(i => i.ClrType, i => new ValueAccessor(i.ClrType));
            _tempColumnNumOrderName = config.TempColumnNumOrderName;
            _shadowProperties = new HashSet<string>(config.EfProperties.Where(i => i.IsShadowProperty()).Select(i => i.Name));
            _convertibleProperties = config.EfProperties.Select(i => new { ValueConverter = i.GetValueConverter(), i.Name }).Where(i => i.ValueConverter != null).ToDictionary(i => i.Name, i => i.ValueConverter);
            _context = config.Context;

            _currentType = null;
            Current = null;
            _memberNames = config.EfProperties.Select(i => i.Name).ToArray();
            if (!string.IsNullOrWhiteSpace(_tempColumnNumOrderName))
                _memberNames = _memberNames.Union(new[] { _tempColumnNumOrderName }).ToArray();
            _effectiveTypes = config.EfProperties.Select(i => Nullable.GetUnderlyingType(i.ClrType) ?? i.ClrType).ToArray();
            _allowNull = config.EfProperties.Select(i => i.IsColumnNullable()).ToArray();
            _source = source.GetEnumerator();
        }

        protected object Current;
        private Type _currentType;


        public int Depth => 0;

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }
        public void Close()
        {
            HasRows = false;
            Current = null;
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
                    Current = tmp.Current;
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
            Current = null;
            return false;
        }

        public int RecordsAffected => 0;

        public void Dispose()
        {
            HasRows = false;
            Current = null;
            _currentType = null;
            _source = null;
        }

        public int FieldCount => _memberNames.Length;

        public bool IsClosed => _source == null;

        public bool GetBoolean(int i) => (bool)this[i];

        public byte GetByte(int i) => (byte)this[i];

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            byte[] s = (byte[])this[i];
            int available = s.Length - (int)fieldOffset;
            if (available <= 0) return 0;

            int count = Math.Min(length, available);
            Buffer.BlockCopy(s, (int)fieldOffset, buffer, bufferoffset, count);
            return count;
        }

        public char GetChar(int i) => (char)this[i];

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            string s = (string)this[i];
            int available = s.Length - (int)fieldoffset;
            if (available <= 0) return 0;

            int count = Math.Min(length, available);
            s.CopyTo((int)fieldoffset, buffer, bufferoffset, count);
            return count;
        }
        public string GetDataTypeName(int i) => (_effectiveTypes == null ? typeof(object) : _effectiveTypes[i]).Name;
        public DateTime GetDateTime(int i) => (DateTime)this[i];
        public decimal GetDecimal(int i) => (decimal)this[i];
        public double GetDouble(int i) => (double)this[i];
        public Type GetFieldType(int i) => _effectiveTypes == null ? typeof(object) : _effectiveTypes[i];
        public float GetFloat(int i) => (float)this[i];
        public Guid GetGuid(int i) => (Guid)this[i];
        public short GetInt16(int i) => (short)this[i];
        public int GetInt32(int i) => (int)this[i];
        public long GetInt64(int i) => (long)this[i];
        public string GetName(int i) => _memberNames[i];
        public int GetOrdinal(string name) => Array.IndexOf(_memberNames, name);
        public string GetString(int i) => (string)this[i];
        public object GetValue(int i) => this[i];
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
                    return etr.Property(name).CurrentValue;
                }
                else
                {
                    if (!_accessorsByType.TryGetValue(_currentType, out var acc)) return DBNull.Value;
                    var val = acc[Current, name];
                    if (val == null) return DBNull.Value;
                    if (_convertibleProperties.TryGetValue(name, out var converter)) return converter.ConvertToProvider(val);
                    return val;
                }
            }
        }
        public object this[int i] => this[_memberNames[i]] ?? DBNull.Value;
    }
    public class ObjectDataReaderConfig
    {
        public IEnumerable<IProperty> EfProperties { get; set; }
        public IEnumerable<IEntityType> Types { get; set; }
        public DbContext Context { get; set; }
        public string TempColumnNumOrderName { get; set; }
    }
}
