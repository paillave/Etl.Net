using Paillave.Etl.Core;
using System;
using System.Collections.Generic;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Data;

namespace Paillave.Etl.SqlServer
{
    public class ToSqlCommandArgs<TIn, TStream, TValue>
        where TIn : class
        where TStream : IStream<TIn>
    {
        public string SqlQuery { get; set; }
        public string ConnectionName { get; set; }
        public TStream SourceStream { get; set; }
        public Func<TIn, TValue> GetValue { get; set; }
    }
    public class ToSqlCommandStreamNode<TIn, TStream, TValue> : StreamNodeBase<TIn, TStream, ToSqlCommandArgs<TIn, TStream, TValue>>
        where TIn : class
        where TStream : IStream<TIn>
    {
        private static IDictionary<string, PropertyInfo> _inPropertyInfos = typeof(TIn).GetProperties().ToDictionary(i => i.Name, StringComparer.InvariantCultureIgnoreCase);
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public ToSqlCommandStreamNode(string name, ToSqlCommandArgs<TIn, TStream, TValue> args) : base(name, args) { }
        protected override TStream CreateOutputStream(ToSqlCommandArgs<TIn, TStream, TValue> args)
        {
            var ret = args.SourceStream.Observable.Do(i => ProcessItem(args.GetValue(i), args.ConnectionName));
            return base.CreateMatchingStream(ret, args.SourceStream);
        }
        public void ProcessItem(TValue item, string connectionName)
        {
            var resolver = this.ExecutionContext.DependencyResolver;
            var sqlConnection = connectionName == null ? resolver.Resolve<IDbConnection>() : resolver.Resolve<IDbConnection>(connectionName);
            var command = sqlConnection.CreateCommand();
            command.CommandText = base.Args.SqlQuery;
            command.CommandType = CommandType.Text;

            // var command = new SqlCommand(base.Args.SqlQuery, sqlConnection);
            Regex getParamRegex = new Regex(@"@(?<param>\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var allMatches = getParamRegex.Matches(base.Args.SqlQuery).ToList().Select(match => match.Groups["param"].Value).Distinct().ToList();
            foreach (var parameterName in allMatches)
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = $"@{parameterName}";
                parameter.Value = _inPropertyInfos[parameterName].GetValue(item) ?? DBNull.Value;
                command.Parameters.Add(parameter);
                // command.Parameters.Add(new SqlParameter($"@{parameterName}", _inPropertyInfos[parameterName].GetValue(item) ?? DBNull.Value));
            }
            command.ExecuteNonQuery();
        }
    }
}
