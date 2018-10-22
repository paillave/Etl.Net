using Paillave.Etl.Core;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Paillave.Etl.SqlServer.ValuesProviders
{
    public class SqlCommandValueProviderArgs<TIn, TOut>
    {
        public string SqlQuery { get; set; }
        public bool NoParallelisation { get; set; } = false;
    }
    public class SqlCommandValueProvider<TIn, TOut> : ValuesProviderBase<TIn, SqlConnection, TOut>
    {
        private static ConstructorInfo _outConstructor = typeof(TOut).GetConstructor(new Type[] { });
        private static IDictionary<string, PropertyInfo> _outPropertyInfos = typeof(TOut).GetProperties().ToDictionary(i => i.Name, StringComparer.InvariantCultureIgnoreCase);
        private static IDictionary<string, PropertyInfo> _inPropertyInfos = typeof(TIn).GetProperties().ToDictionary(i => i.Name, StringComparer.InvariantCultureIgnoreCase);
        private SqlCommandValueProviderArgs<TIn, TOut> _args;
        public SqlCommandValueProvider(SqlCommandValueProviderArgs<TIn, TOut> args) : base(args.NoParallelisation)
        {
            _args = args;
        }
        private TOut CreateRecord(SqlDataReader record)
        {
            TOut ret = (TOut)_outConstructor.Invoke(null);
            for (int i = 0; i < record.FieldCount; i++)
                _outPropertyInfos[record.GetName(i)].SetValue(ret, Convert.ChangeType(record.GetValue(i), record.GetFieldType(i)));
            return ret;
        }
        protected override void PushValues(SqlConnection resource, TIn input, Action<TOut> pushValue)
        {
            var command = new SqlCommand(_args.SqlQuery, resource);
            Regex getParamRegex = new Regex(@"(?<param>@\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var allMatches = getParamRegex.Matches(_args.SqlQuery);
            foreach (var match in allMatches)
            {
                string parameterName = match.ToString();
                command.Parameters.Add(new SqlParameter($"{parameterName}", _inPropertyInfos[parameterName].GetValue(input)));
            }

            using (var reader = command.ExecuteReader())
                while (reader.Read())
                    pushValue(CreateRecord(reader));
        }
    }
}
