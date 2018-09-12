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
    public class ToSqlCommandArgs<TIn, TStream>
        where TIn : class
        where TStream : IStream<TIn>
    {
        public string SqlQuery { get; set; }
        public TStream SourceStream { get; set; }
        public IStream<SqlConnection> SqlConnectionStream { get; set; }
    }
    public class ToSqlCommandStreamNode<TIn, TStream> : StreamNodeBase<TIn, TStream, ToSqlCommandArgs<TIn, TStream>>
        where TIn : class
        where TStream : IStream<TIn>
    {
        private static IDictionary<string, PropertyInfo> _inPropertyInfos = typeof(TIn).GetProperties().ToDictionary(i => i.Name, StringComparer.InvariantCultureIgnoreCase);
        public override bool IsAwaitable => true;
        public ToSqlCommandStreamNode(string name, ToSqlCommandArgs<TIn, TStream> args) : base(name, args)
        {
        }

        protected override TStream CreateOutputStream(ToSqlCommandArgs<TIn, TStream> args)
        {
            var dbContextStream = args.SqlConnectionStream.Observable.First();
            var ret = args.SourceStream.Observable
                .CombineWithLatest(dbContextStream, (i, c) => new { Connection = c, Item = i }, true)
                .Do(i => ProcessItem(i.Item, i.Connection))
                .Map(i => i.Item);
            return base.CreateMatchingStream(ret, args.SourceStream);
        }
        public void ProcessItem(TIn item, SqlConnection sqlConnection)
        {
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
