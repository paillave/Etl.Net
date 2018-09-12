using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Paillave.Etl.Core
{
    public class ObjectBuilder<TOut>
    {
        private static readonly ParameterInfo[] _anonymousConstructorParameters;
        private static readonly IDictionary<string, PropertyInfo> _nonAnonymousPropertyInfos;
        private static readonly bool _isOutputAnonymous;
        private static Type _outType = typeof(TOut);
        static ObjectBuilder()
        {
            _isOutputAnonymous = Attribute.IsDefined(_outType, typeof(CompilerGeneratedAttribute), false);
            if (_isOutputAnonymous)
                _anonymousConstructorParameters = _outType.GetConstructors()[0].GetParameters();
            else
                _nonAnonymousPropertyInfos = _outType.GetProperties().ToDictionary(i => i.Name);
        }
        public IDictionary<string, object> Values { get; private set; }
        public ObjectBuilder(bool presetDefaultValues = false)
        {
            if (presetDefaultValues)
            {
                if (_isOutputAnonymous)
                {
                    Values = _anonymousConstructorParameters.ToDictionary(i => i.Name, i => Activator.CreateInstance(i.ParameterType));
                }
                else
                {
                    Values = _nonAnonymousPropertyInfos.ToDictionary(i => i.Key, i => Activator.CreateInstance(i.Value.PropertyType));
                }
            }
            else
                Values = new Dictionary<string, object>();
        }
        public TOut CreateInstance()
        {
            return CreateInstance(this.Values);
        }
        public static TOut CreateInstance(IDictionary<string, object> values)
        {
            TOut ret;
            if (_isOutputAnonymous)
            {
                ret = (TOut)Activator.CreateInstance(_outType, _anonymousConstructorParameters.Select(i => values[i.Name]).ToArray());
            }
            else
            {
                ret = Activator.CreateInstance<TOut>();
                foreach (var aggregator in values)
                    _nonAnonymousPropertyInfos[aggregator.Key].SetValue(ret, aggregator.Value);
            }
            return ret;
        }

    }
}
