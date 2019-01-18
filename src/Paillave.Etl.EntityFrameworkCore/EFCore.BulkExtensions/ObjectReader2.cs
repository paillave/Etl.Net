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

namespace Paillave.Etl.EntityFrameworkCore.EFCore.BulkExtensions
{
    public class ObjectReader2 : IDataReader
    {
        private IEnumerator _source;
        private readonly Dictionary<Type, TypeAccessor> _accessors = new Dictionary<Type, TypeAccessor>();
        private readonly HashSet<string> _shadowProperties;
        private readonly Dictionary<string, ValueConverter> _convertibleProperties;
        private readonly DbContext _context;
        private readonly string[] _memberNames;
        private readonly Type[] _effectiveTypes;
        private readonly bool[] _allowNull;

        public ObjectReader2(IEnumerable source, IProperty[] efProperties, Type[] types, HashSet<string> shadowProperties, Dictionary<string, ValueConverter> convertibleProperties, DbContext context)
        {
            if (source == null) throw new ArgumentOutOfRangeException("source");

            _accessors = types.ToDictionary(i => i, i => TypeAccessor.Create(i));

            _shadowProperties = shadowProperties;
            _convertibleProperties = convertibleProperties;
            _context = context;

            _currentType = null;
            Current = null;
            _memberNames = efProperties.Select(i => i.Name).ToArray();
            _effectiveTypes = efProperties.Select(i => Nullable.GetUnderlyingType(i.ClrType) ?? i.ClrType).ToArray();
            _allowNull = efProperties.Select(i => i.IsColumnNullable()).ToArray();
            _source = source.GetEnumerator();
        }

        protected object Current;
        private Type _currentType;


        public int Depth => 0;

        public DataTable GetSchemaTable()
        {
            // these are the columns used by DataTable load
            DataTable table = new DataTable
            {
                Columns =
                {
                    {"ColumnOrdinal", typeof(int)},
                    {"ColumnName", typeof(string)},
                    {"DataType", typeof(Type)},
                    {"ColumnSize", typeof(int)},
                    {"AllowDBNull", typeof(bool)}
                }
            };
            object[] rowData = new object[5];
            for (int i = 0; i < _memberNames.Length; i++)
            {
                rowData[0] = i;
                rowData[1] = _memberNames[i];
                rowData[2] = _effectiveTypes == null ? typeof(object) : _effectiveTypes[i];
                rowData[3] = -1;
                rowData[4] = _allowNull == null ? true : _allowNull[i];
                table.Rows.Add(rowData);
            }
            return table;
        }
        public void Close()
        {
            active = false;
            Current = null;
            _currentType = null;
            _source = null;
        }

        public bool HasRows => active;

        private bool active = true;
        public bool NextResult()
        {
            active = false;
            return false;
        }
        public bool Read()
        {
            if (active)
            {
                var tmp = _source;
                if (tmp != null && tmp.MoveNext())
                {
                    Current = tmp.Current;
                    _currentType = Current.GetType();
                    return true;
                }
                else
                {
                    active = false;
                }
            }
            _currentType = null;
            Current = null;
            return false;
        }

        public int RecordsAffected => 0;

        public void Dispose()
        {
            active = false;
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

        //protected override DbDataReader GetDbDataReader(int i)
        //{
        //    throw new NotSupportedException();
        //}

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

        //public override IEnumerator GetEnumerator() => new DbEnumerator(this);

        public int GetValues(object[] values)
        {
            // duplicate the key fields on the stack
            var members = this._memberNames;
            var current = this.Current;
            var accessor = _accessors[_currentType];

            int count = Math.Min(values.Length, members.Length);
            for (int i = 0; i < count; i++) values[i] = accessor[current, members[i]] ?? DBNull.Value;
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
                if (_shadowProperties.Contains(name))
                {
                    var etr = _context.Entry(Current);
                    return etr.Property(name).CurrentValue;
                }
                else if (_convertibleProperties.TryGetValue(name, out var converter))
                {
                    var etr = _context.Entry(Current);
                    var currentValue = etr.Property(name).CurrentValue;
                    return converter.ConvertToProvider(currentValue);
                }
                return _accessors[_currentType][Current, name] ?? DBNull.Value;
            }
        }

        public object this[int i] => this[_memberNames[i]] ?? DBNull.Value;
    }
}
