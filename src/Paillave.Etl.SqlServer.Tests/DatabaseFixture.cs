using Microsoft.Data.SqlClient;
using System;
using System.Data.Odbc;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Paillave.Etl.SqlServer.Tests;


[CollectionDefinition("MSSQLLocalDB")]
public sealed class DatabaseCollection : ICollectionFixture<DatabaseFixture>{ }


public sealed class DatabaseFixture : IAsyncLifetime
{
    public string DatabaseName { get; } = $"Test_{Guid.CreateVersion7():N}";
    private const string ConnectionStringMaster = @"Server=(localdb)\MSSQLLocalDB;Database=master;Integrated Security=True;TrustServerCertificate=True;";

    public async Task InitializeAsync()
    {
        await CreateDatabaseAsync();
        await CreateSchemaAsync();
        await ResetSeedDataAsync();
    }


    public SqlConnection CreateConnection() => new SqlConnection($@"Server=(localdb)\MSSQLLocalDB;Database={DatabaseName};Integrated Security=True;TrustServerCertificate=True;");
    public OdbcConnection CreateOdbcConnection() => new OdbcConnection($@"Driver={{ODBC Driver 18 for SQL Server}};Server=(localdb)\MSSQLLocalDB;Database={DatabaseName};Trusted_Connection=Yes;TrustServerCertificate=Yes;");


    private async Task CreateDatabaseAsync()
    {
        await using var connection = new SqlConnection(ConnectionStringMaster);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"create database [{DatabaseName}]";
        await command.ExecuteNonQueryAsync();
    }


    private async Task CreateSchemaAsync()
    {
        var sql = await File.ReadAllTextAsync(@"SqlServer\Schema.sql");

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }


    public async Task ResetSeedDataAsync()
    {
        var sql = await File.ReadAllTextAsync(@"SqlServer\ResetSeedData.sql");

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync();
    }


    public async Task DisposeAsync()
    {
        await using var connection = new SqlConnection(ConnectionStringMaster);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            alter database [{DatabaseName}] set single_user with rollback immediate;
            drop database [{DatabaseName}];
            """;

        await command.ExecuteNonQueryAsync();
    }
}