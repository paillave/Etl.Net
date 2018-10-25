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
    public class SqlCommandValueProvider<TIn, TOut>
    {
        private static ConstructorInfo _outConstructor = typeof(TOut).GetConstructor(new Type[] { });
        private static IDictionary<string, PropertyInfo> _outPropertyInfos = typeof(TOut).GetProperties().ToDictionary(i => i.Name, StringComparer.InvariantCultureIgnoreCase);
        private static IDictionary<string, PropertyInfo> _inPropertyInfos = typeof(TIn).GetProperties().ToDictionary(i => i.Name, StringComparer.InvariantCultureIgnoreCase);

        private string _sqlQuery;
        public SqlCommandValueProvider(string sqlQuery)
        {
            _sqlQuery = sqlQuery;
        }
        private TOut CreateRecord(SqlDataReader record)
        {
            TOut ret = (TOut)_outConstructor.Invoke(null);
            for (int i = 0; i < record.FieldCount; i++)
                _outPropertyInfos[record.GetName(i)].SetValue(ret, Convert.ChangeType(record.GetValue(i), record.GetFieldType(i)));
            return ret;
        }
        public void PushValues(TIn input, SqlConnection resource, Action<TOut> pushValue)
        {
            var command = new SqlCommand(_sqlQuery, resource);
            Regex getParamRegex = new Regex(@"(?<param>@\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var allMatches = getParamRegex.Matches(_sqlQuery);
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
