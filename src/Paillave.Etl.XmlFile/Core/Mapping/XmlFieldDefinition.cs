using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Paillave.Etl.XmlFile.Core.Mapping
{
    public class XmlFieldDefinition
    {
        static XmlFieldDefinition()
        {
            _typeConverters = typeof(XmlConvert)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(i => i.Name.StartsWith("To") && i.GetParameters().Count() == 1 && i.GetParameters()[0].ParameterType == typeof(string))
                .ToDictionary(i => i.ReturnType);
        }
        private MethodInfo _convertMethod = null;
        private static readonly Dictionary<Type, MethodInfo> _typeConverters;
        public int DepthScope { get; internal set; } = 0;
        public string NodePath { get; internal set; } = null;
        public bool ForSourceName { get; internal set; } = false;
        public bool ForRowGuid { get; internal set; } = false;
        private PropertyInfo _targetPropertyInfo = null;
        public PropertyInfo TargetPropertyInfo
        {
            get => _targetPropertyInfo;
            internal set
            {
                _targetPropertyInfo = value;
                var underlyingType = Nullable.GetUnderlyingType(_targetPropertyInfo.PropertyType);
                IsNullableProperty = underlyingType != null;
                if (!IsNullableProperty)
                    underlyingType = _targetPropertyInfo.PropertyType;
                if (underlyingType == typeof(string))
                    _convertMethod = null;
                else
                    _convertMethod = _typeConverters[underlyingType];
            }
        }
        public bool IsNullableProperty { get; private set; }
        public object Convert(string stringValue)
        {
            if (_convertMethod != null)
                return _convertMethod.Invoke(null, new object[] { stringValue });
            else return stringValue;
        }
    }
}
