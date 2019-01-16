using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace EFCore.BulkExtensions
{
    public class ListDataReader<T> : IDataReader
            //where T : class
    {
        #region Private fields
        private IEnumerator<T> _iterator;
        private List<PropertyInfo> _properties = new List<PropertyInfo>();
        #endregion

        #region Public properties
        #endregion

        #region Constructors
        public ListDataReader(IEnumerable<T> list)
        {
            this._iterator = list.GetEnumerator();

            _properties.AddRange(
                typeof(T).GetProperties(
                    BindingFlags.GetProperty |
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.DeclaredOnly
                    )
                );
        }
        #endregion

        #region IDataReader implementation
        public int Depth { get; }
        public bool IsClosed { get; }
        public int RecordsAffected { get; }

        public void Close()
        {
            _iterator.Dispose();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            return _iterator.MoveNext();
        }
        #endregion

        #region IDisposable implementation
        public void Dispose()
        {
            Close();
        }
        #endregion

        #region IDataRecord
        public string GetName(int i)
        {
            return _properties[i].Name;
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            return _properties[i].PropertyType;
        }

        public object GetValue(int i)
        {
            return _properties[i].GetValue(_iterator.Current, null);
        }

        public int GetValues(object[] values)
        {
            int numberOfCopiedValues = Math.Max(_properties.Count, values.Length);

            for (int i = 0; i < numberOfCopiedValues; i++)
            {
                values[i] = GetValue(i);
            }

            return numberOfCopiedValues;
        }

        public int GetOrdinal(string name)
        {
            var index = _properties.FindIndex(p => p.Name == name);

            return index;
        }
        #region get typed properties
        public bool GetBoolean(int i)
        {
            return (bool)GetValue(i);
        }

        public byte GetByte(int i)
        {
            return (byte)GetValue(i);
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            return (char)GetValue(i);
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            return (Guid)GetValue(i);
        }

        public short GetInt16(int i)
        {
            return (short)GetValue(i);
        }

        public int GetInt32(int i)
        {
            return (int)GetValue(i);
        }

        public long GetInt64(int i)
        {
            return (long)GetValue(i);
        }

        public float GetFloat(int i)
        {
            return (float)GetValue(i);
        }

        public double GetDouble(int i)
        {
            return (double)GetValue(i);
        }

        public string GetString(int i)
        {
            return (string)GetValue(i);
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)GetValue(i);
        }

        public DateTime GetDateTime(int i)
        {
            return (DateTime)GetValue(i);
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return GetValue(i) == null;
        }
        #endregion

        public int FieldCount
        {
            get { return _properties.Count; }
        }

        object IDataRecord.this[int i]
        {
            get { return GetValue(i); }
        }

        object IDataRecord.this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }
        #endregion
    }
}
