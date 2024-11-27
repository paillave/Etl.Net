using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Paillave.Etl.Core
{
    public class GroupProcessor
    {
        public static GroupProcessor<TIn, TMultiKey> Create<TIn, TMultiKey>(Func<TIn, TMultiKey> getKeys) => new GroupProcessor<TIn, TMultiKey>(getKeys);
    }
    public class GroupProcessor<TIn, TMultiKey>(Func<TIn, TMultiKey> getKeys)
    {
        private Func<TIn, TMultiKey> _getKeys = getKeys;
        private static PropertyInfo[] _keyProperties = typeof(TMultiKey).GetProperties().ToArray();
        private Dictionary<(string PropertyName, object Value), (ObjectBuilder<TMultiKey> key, List<TIn>)> _keysDictionaries = new Dictionary<(string PropertyName, object Value), (ObjectBuilder<TMultiKey> key, List<TIn> rows)>();
        private List<TIn> _rowsWithNoKey = new List<TIn>();

        public void ProcessRow(TIn row)
        {
            var keys = _getKeys(row);
            var keyValues = _keyProperties
                .Select(kp => new
                {
                    PropertyName = kp.Name,
                    PropertyValue = kp.GetValue(keys)
                }).ToList();
            var keyElements = keyValues
                .Where(i => i.PropertyValue != null)
                .Select(kp => GetValue(kp.PropertyName, kp.PropertyValue))
                .ToList();
            var toMerge = keyElements.GroupBy(i => i.key, (k, i) => i.First()).ToList();
            if (toMerge.Count() == 0)
            {
                _rowsWithNoKey.Add(row);
            }
            else if (toMerge.Count() > 1)
            {
                var merged = toMerge.First();
                foreach (var item in toMerge.Skip(1))
                {
                    merged.key.Merge(item.key);
                    merged = (merged.key, merged.rows.Union(item.rows).Distinct().ToList());
                }
                foreach (var item in merged.key.Values)
                {
                    _keysDictionaries[(item.Key, item.Value)] = merged;
                }
                merged.rows.Add(row);
            }
            else
            {
                foreach (var item in toMerge)
                {
                    item.rows.Add(row);
                }
            }
        }
        private (ObjectBuilder<TMultiKey> key, List<TIn> rows) GetValue(string propertyName, object value)
        {
            if (_keysDictionaries.TryGetValue((propertyName, value), out var ret))
            {
                return ret;
            }
            var key = new ObjectBuilder<TMultiKey>(true);
            key.Values[propertyName] = value;
            var elt = (key, new List<TIn>());
            _keysDictionaries[(propertyName, value)] = elt;
            return elt;
        }
        public Dictionary<TMultiKey, List<TIn>> GetGroups()
        {
            var tmp = _keysDictionaries.GroupBy(i => i.Value.key, (k, i) => new { Key = k.CreateInstance(), Rows = i.First().Value.Item2 }).ToDictionary(i => i.Key, i => i.Rows);
            if (this._rowsWithNoKey.Count > 0)
            {
                tmp[new ObjectBuilder<TMultiKey>(true).CreateInstance()] = this._rowsWithNoKey;
            }
            return tmp;
        }
    }
}