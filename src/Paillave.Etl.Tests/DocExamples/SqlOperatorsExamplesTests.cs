using System;
using System.Collections.Concurrent;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.SqlServer;
using Xunit;

namespace Paillave.Etl.Tests.DocExamples;

/// <summary>
/// Examples for <c>documentation/docs/operators/5_sql.md</c>.
///
/// `Paillave.Etl.SqlServer` exposes three operators that all sit on
/// top of a plain <see cref="IDbConnection"/> resolved from the DI
/// container — only <c>SqlServerSave</c> emits T-SQL specifically and
/// is therefore not exercised by these SQLite-based tests.
/// </summary>
public class SqlOperatorsExamplesTests
{
    public class CountryRow
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class CountryInput
    {
        public string Code { get; set; } = "";
    }

    private static (IServiceProvider services, SqliteConnection keepAlive) BuildSqlite(Action<IDbConnection>? seed = null)
    {
        var dbName = "sql_" + Guid.NewGuid().ToString("N");
        var connectionString = $"DataSource=file:{dbName}?mode=memory&cache=shared";
        var keepAlive = new SqliteConnection(connectionString);
        keepAlive.Open();

        using (var seedCnx = new SqliteConnection(connectionString))
        {
            seedCnx.Open();
            using var cmd = seedCnx.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE Country(Code TEXT PRIMARY KEY, Name TEXT NOT NULL);
                INSERT INTO Country(Code, Name) VALUES ('FR','France'), ('DE','Germany'), ('IT','Italy');
            ";
            cmd.ExecuteNonQuery();
            seed?.Invoke(seedCnx);
        }

        var services = new ServiceCollection()
            .AddTransient<IDbConnection>(_ =>
            {
                var c = new SqliteConnection(connectionString);
                c.Open();
                return c;
            })
            .BuildServiceProvider();

        return (services, keepAlive);
    }

    // ===================================================================
    // CrossApplySqlServerQuery — read with mapping
    // ===================================================================

    [Fact]
    public async Task CrossApplySqlServerQuery_ReadsRowsAndMapsToType()
    {
        var (services, keepAlive) = BuildSqlite();
        using var _ = keepAlive;

        var collected = new ConcurrentBag<CountryRow>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            "go",
            root => root
                .CrossApply("trigger", _ => new[] { new { Filter = "F" } })
                .CrossApplySqlServerQuery("read",
                    o => o.FromQuery("SELECT Code, Name FROM Country WHERE Code LIKE @Filter || '%' ORDER BY Code")
                          .WithMapping<CountryRow>(i => new CountryRow
                          {
                              Code = i.ToColumn<string>("Code"),
                              Name = i.ToColumn<string>("Name"),
                          }))
                .Do("collect", collected.Add),
            new ExecutionOptions<string> { Services = services });

        Assert.False(status.Failed);
        Assert.Single(collected);
        var only = Assert.Single(collected);
        Assert.Equal("FR", only.Code);
        Assert.Equal("France", only.Name);
    }

    // ===================================================================
    // CrossApplySqlServerQuery — default mapping (column → property by name)
    // ===================================================================

    [Fact]
    public async Task CrossApplySqlServerQuery_DefaultMapping_MatchesByPropertyName()
    {
        var (services, keepAlive) = BuildSqlite();
        using var _ = keepAlive;

        var collected = new ConcurrentBag<CountryRow>();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            "go",
            root => root
                .CrossApply("trigger", _ => new[] { new { Code = "DE" } })
                .CrossApplySqlServerQuery("read",
                    o => o.FromQuery("SELECT Code, Name FROM Country WHERE Code = @Code")
                          .WithMapping<CountryRow>())
                .Do("collect", collected.Add),
            new ExecutionOptions<string> { Services = services });

        Assert.False(status.Failed);
        var only = Assert.Single(collected);
        Assert.Equal("DE", only.Code);
        Assert.Equal("Germany", only.Name);
    }

    // ===================================================================
    // ToSqlCommand — execute a parameterised statement per row
    // ===================================================================

    [Fact]
    public async Task ToSqlCommand_ExecutesStatementPerRow()
    {
        var (services, keepAlive) = BuildSqlite();
        using var _ = keepAlive;

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            "go",
            root => root
                .CrossApply("rows", _ => new[]
                {
                    new CountryRow { Code = "ES", Name = "Spain" },
                    new CountryRow { Code = "PT", Name = "Portugal" },
                })
                .ToSqlCommand("insert",
                    "INSERT INTO Country(Code, Name) VALUES(@Code, @Name) RETURNING Code, Name"),
            new ExecutionOptions<string> { Services = services });

        Assert.False(status.Failed);

        // verify they really landed in the table
        using var cnx = (SqliteConnection)services.GetRequiredService<IDbConnection>();
        using var cmd = cnx.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Country";
        Assert.Equal(5L, (long)cmd.ExecuteScalar()!);
    }

    // Note: `WithKeyedConnection("name")` is supported by the operator API but
    // requires a keyed-aware `IServiceProvider`. The runner currently wraps the
    // user-provided provider in a `CompositeServiceProvider` that does not forward
    // keyed lookups, so the keyed-connection scenario is documented but not exercised
    // here.
}
