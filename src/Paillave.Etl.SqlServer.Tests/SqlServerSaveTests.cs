using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.SqlServer;
using Paillave.Etl.SqlServer.Tests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Paillave.Etl.Tests.SqlServer;

[Collection("MSSQLLocalDB")]
public sealed class SqlServerSaveTests(DatabaseFixture databaseFixture)
{
    private readonly IEnumerable<Person> peopleSource = [
        new(1, "Thomas", "Bangaltar", null, "Robot",         new Expectation(Id: 1, WasInserted: false, WasChanged: true)),
        new(2, "Guy-Manuel", "Homem-Christo", "de", "Robot", new Expectation(Id: 2, WasInserted: false, WasChanged: true)),
        new(3, "Nicolas", "Godin", null, "Sexy boy",         new Expectation(Id: 3, WasInserted: false, WasChanged: false)),
        new(4, "Jean-Benoît", "Dunckel", null, "Sexy boy",   new Expectation(Id: 4, WasInserted: false, WasChanged: false)),
        new(null, "Laurent", "Garnier", null, "Techno god",  new Expectation(Id: 5, WasInserted: true,  WasChanged: true)),
    ];

    private readonly Groups[] groupsSource = [ new(1, "Daft Punk"), new(4, "Air")];


    [Fact]
    public async Task Insert_ReservedColumnName() => await ProcessInsert(databaseFixture.CreateConnection, groupsSource);


    [Fact]
    public async Task Insert_ReservedColumnName_OdbC() => await ProcessInsert(databaseFixture.CreateOdbcConnection, groupsSource);


    [Fact(DisplayName = "Table name is derived from item type if unspecified")]
    public async Task Insert_TableNameFromType()
    {
        await databaseFixture.ResetSeedDataAsync();

        var executionOptions = new ExecutionOptions<IEnumerable<Groups>>
        {
            Services = new ServiceCollection().AddTransient<IDbConnection>(_ => databaseFixture.CreateConnection()).BuildServiceProvider(),
        };

        var result = await StreamProcessRunner.CreateAndExecuteAsync(
            groupsSource,
            contextStream => contextStream
                .CrossApply("Create values from enumeration", context => context)
                .SqlServerSave("Insert into table matching items’ type name"),
            executionOptions);

        Assert.False(result.Failed, $"Process failed: {result.ErrorTraceEvent?.NodeName} ({result.ErrorTraceEvent?.NodeTypeName}): {result.ErrorTraceEvent?.Content?.Message}");

        await AssertGroupsInDatabaseEqual(groupsSource);
    }


    [Fact]
    public async Task Upsert_WithReadBack() => await ProcessUpsert(databaseFixture.CreateConnection);


    [Fact]
    public async Task Upsert_WithReadBack_Odbc() => await ProcessUpsert(databaseFixture.CreateOdbcConnection);

    
    private async Task ProcessInsert(Func<IDbConnection> connectionFactory, IEnumerable<Groups> sourceData)
    {
        await databaseFixture.ResetSeedDataAsync();

        var executionOptions = new ExecutionOptions<IEnumerable<Groups>>
        {
            Services = new ServiceCollection().AddTransient(_ => connectionFactory()).BuildServiceProvider(),
        };

        var result = await StreamProcessRunner.CreateAndExecuteAsync(
            sourceData,
            contextStream => contextStream
                .CrossApply("Create values from enumeration", context => context)
                .SqlServerSave("Insert", m => m
                    .ToTable("Groups")),
            executionOptions);

        Assert.False(result.Failed, $"Process failed: {result.ErrorTraceEvent?.NodeName} ({result.ErrorTraceEvent?.NodeTypeName}): {result.ErrorTraceEvent?.Content?.Message}");

        await AssertGroupsInDatabaseEqual(sourceData);
    }


    private async Task ProcessUpsert(Func<IDbConnection> connectionFactory)
    {
        await databaseFixture.ResetSeedDataAsync();
        var startTimeUtc = DateTime.UtcNow;

        var executionOptions = new ExecutionOptions<IEnumerable<Person>>
        {
            Services = new ServiceCollection().AddTransient(_ => connectionFactory()).BuildServiceProvider(),
        };

        var result = await StreamProcessRunner.CreateAndExecuteAsync(
            peopleSource,
            contextStream => contextStream
                .CrossApply("Create values from enumeration", context => context)
                .SqlServerSave("Upsert via Id", m => m
                    .ToTable("People")
                    .DoNotSave(p => new { p.Id, p.InsertedAtUtc, p.ValidFromUtc, p.ValidToUtc, p.Expectation })
                    .SeekOn(p => p.Id!)
                    .ReadBackChanges()),
            executionOptions);
        
        Assert.False(result.Failed, result.Failed ? $"Process failed: {result.ErrorTraceEvent.NodeName} ({result.ErrorTraceEvent.NodeTypeName}): {result.ErrorTraceEvent.Content.Message}" : null);

        foreach (var p in peopleSource)
        {
            await AssertDatabaseRowAsExpected(p, startTimeUtc);
            AssertItemPopulatedWithDatabaseOutput(p);
        }
    }
    

    private static void AssertItemPopulatedWithDatabaseOutput(Person person)
    {
        Assert.Equal(person.Expectation.Id, person.Id);
        Assert.NotNull(person.InsertedAtUtc);
        Assert.NotNull(person.ValidFromUtc);
        Assert.NotNull(person.ValidToUtc);
    }


    private async Task AssertDatabaseRowAsExpected(Person person, DateTime startTimeUtc)
    {
        await using var connection = databaseFixture.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = "select * from People where Id = @Id;";
        command.Parameters.AddWithValue("Id", person.Id);

        using var reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync(), $"Expected row with Id {person.Id} was not found in the database.");
        Assert.Equal(person.Firstname, reader.GetString(nameof(Person.Firstname)));
        Assert.Equal(person.Lastname, reader.GetString(nameof(Person.Lastname)));
        Assert.Equal(person.LastnamePrefix, reader.IsDBNull(nameof(Person.LastnamePrefix)) ? null : reader.GetString("LastnamePrefix"));
        Assert.Equal(person.Role, reader.GetString(nameof(Person.Role)));

        if (person.Expectation.WasInserted)
            Assert.True(reader.GetDateTime("InsertedAtUtc") > startTimeUtc, $"Expected row with Id {person.Id} to have database-generated timestamp.");
        else
            Assert.True(reader.GetDateTime("InsertedAtUtc") < startTimeUtc, $"Existing row with Id {person.Id} has newer timestamp than expected???");

        //https://github.com/paillave/Etl.Net/issues/575
        //if (person.Expectation.WasChanged)
        //    Assert.True(reader.GetDateTime("ValidFromUtc") > startTimeUtc, $"Row with Id {person.Id} was expected to change, but didn’t trigger system-versioning.");
        //else
        //    Assert.True(reader.GetDateTime("ValidFromUtc") < startTimeUtc, $"Row without changes (Id {person.Id}) received an UPDATE and triggered system-versioning.");
    }


    private async Task AssertGroupsInDatabaseEqual(IEnumerable<Groups> expectedData)
    {
        await using var connection = databaseFixture.CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"select * from {nameof(Groups)} order by PersonId";
        using var reader = await command.ExecuteReaderAsync();
        var actualData = new List<Groups>();
        while (await reader.ReadAsync())
            actualData.Add(new(reader.GetInt32(nameof(Groups.PersonId)), reader.GetString(nameof(Groups.Group))));

        Assert.True(Enumerable.SequenceEqual(expectedData.OrderBy(g => g.PersonId), actualData), "Groups in database do not match Groups sent.");
    }
}


internal record Expectation(int Id, bool WasInserted, bool WasChanged);

internal class Person(int? Id, string Firstname, string Lastname, string? LastnamePrefix, string? Role, Expectation Expectation)
{
    public int? Id { get; set; } = Id;
    public string Firstname { get; set; } = Firstname;
    public string Lastname { get; set; } = Lastname;
    public string? LastnamePrefix { get; set; } = LastnamePrefix;
    public string? Role { get; set; } = Role;
    public DateTime? InsertedAtUtc { get; set; } = null;
    public DateTime? ValidFromUtc { get; set; } = null;
    public DateTime? ValidToUtc { get; set; } = null;
    public Expectation Expectation { get; } = Expectation;
}

internal record Groups(int PersonId, string Group);