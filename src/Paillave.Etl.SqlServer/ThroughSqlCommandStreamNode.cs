using Paillave.Etl.Core;
using Paillave.Etl.Core.Streams;
using Paillave.Etl.Reactive.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.Etl.Reactive.Operators;
using System.Linq;
using System.Data.SqlClient;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Paillave.Etl.SqlServer.StreamNodes
{
    public class ThroughSqlCommandArgs<TIn, TStream>
        where TIn : class
        where TStream : IStream<TIn>
    {
        public string SqlQuery { get; set; }
        public string ConnectionName { get; set; }
        public TStream SourceStream { get; set; }
    }
    public class ThroughSqlCommandStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ThroughSqlCommandArgs<TIn, TStream>>
        where TIn : class
        where TStream : IStream<TIn>
    {
        private static IDictionary<string, PropertyInfo> _inPropertyInfos = typeof(TIn).GetProperties().ToDictionary(i => i.Name, StringComparer.InvariantCultureIgnoreCase);
        public override ProcessImpact PerformanceImpact => ProcessImpact.Heavy;
        public override ProcessImpact MemoryFootPrint => ProcessImpact.Light;
        public ThroughSqlCommandStreamNode(string name, ThroughSqlCommandArgs<TIn, TStream> args) : base(name, args) { }
        protected override TStream CreateOutputStream(ThroughSqlCommandArgs<TIn, TStream> args)
        {
            var ret = args.SourceStream.Observable.Do(i => ProcessItem(i, args.ConnectionName));
            return base.CreateMatchingStream(ret, args.SourceStream);
        }
        public void ProcessItem(TIn item, string connectionName)
        {
            var resolver = this.ExecutionContext.DependencyResolver;
            var sqlConnection = connectionName == null ? resolver.Resolve<SqlConnection>() : resolver.Resolve<SqlConnection>(connectionName);
            var command = new SqlCommand(base.Args.SqlQuery, sqlConnection);
            Regex getParamRegex = new Regex(@"(?<param>@\w*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var allMatches = getParamRegex.Matches(base.Args.SqlQuery);
            foreach (var match in allMatches)
            {
                string parameterName = match.ToString();
                command.Parameters.Add(new SqlParameter($"@{parameterName}", _inPropertyInfos[parameterName].GetValue(item)));
            }
            command.ExecuteNonQuery();
        }
    }
}
