using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Paillave.Etl.Core;
using Paillave.Etl.EntityFrameworkCore;
using Xunit;

namespace Paillave.Etl.Tests.DocExamples;

/// <summary>
/// Each test in this file is a runnable mirror of an example from the
/// official documentation under <c>documentation/docs/operators/4_entityFramework.md</c>.
/// All tests rely on a shared SQLite in-memory database so that the
/// snippets shipped to users compile and run end-to-end against a real
/// EF Core relational provider.
/// </summary>
public class EntityFrameworkOperatorsExamplesTests
{
    // ===================================================================
    // Domain model used by every example
    // ===================================================================

    public class Country
    {
        [Key] public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public class Person
    {
        [Key] public int Id { get; set; }
        public string Email { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int CountryId { get; set; }
    }

    public class DocDbContext(DbContextOptions<DocDbContext> options) : DbContext(options)
    {
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<Person> People => Set<Person>();
    }

    // ===================================================================
    // Test harness — one open SQLite connection keeps the in-memory
    // database alive for the whole test, then a service collection is
    // built that exposes a DbContext to ETL.Net.
    // ===================================================================

    private static async Task<(ExecutionStatus status, DocDbContext finalCtx, SqliteConnection cnx)> RunAsync<TConfig>(
        TConfig config,
        Action<DocDbContext>? seed,
        Action<ISingleStream<TConfig>> jobDefinition)
    {
        // Shared-cache in-memory SQLite: one keep-alive connection holds the
        // DB while any number of parallel DbContext instances each open their
        // own connection to the same shared cache.
        var dbName = "doc_" + Guid.NewGuid().ToString("N");
        var connectionString = $"DataSource=file:{dbName}?mode=memory&cache=shared";
        var keepAlive = new SqliteConnection(connectionString);
        keepAlive.Open();
        var optsBuilder = new DbContextOptionsBuilder<DocDbContext>().UseSqlite(connectionString);

        using (var seedCtx = new DocDbContext(optsBuilder.Options))
        {
            seedCtx.Database.EnsureCreated();
            seed?.Invoke(seedCtx);
            seedCtx.SaveChanges();
        }

        var services = new ServiceCollection()
            .AddTransient<DocDbContext>(_ => new DocDbContext(optsBuilder.Options))
            .AddTransient<DbContext>(sp => sp.GetRequiredService<DocDbContext>())
            .BuildServiceProvider();

        var status = await StreamProcessRunner.CreateAndExecuteAsync(
            config,
            jobDefinition,
            new ExecutionOptions<TConfig> { Services = services });

        if (status.Failed && status.ErrorTraceEvent?.Content is UnhandledExceptionStreamTraceContent err)
            throw new Exception($"Pipeline failed in node '{status.ErrorTraceEvent.NodeName}': {err.Message}\n{err.Exception}");

        // Return a fresh context tied to the same connection so callers can assert the final state.
        var finalCtx = new DocDbContext(optsBuilder.Options);
        return (status, finalCtx, keepAlive);
    }

    // ===================================================================
    // EfCoreSelect — read rows from the database into the stream
    // ===================================================================

    [Fact]
    public async Task EfCoreSelect_LoadsAllRowsFromTable()
    {
        var collected = new ConcurrentBag<string>();

        var (status, ctx, cnx) = await RunAsync(
            "go",
            seed =>
            {
                seed.Countries.AddRange(
                    new Country { Code = "FR", Name = "France" },
                    new Country { Code = "DE", Name = "Germany" },
                    new Country { Code = "IT", Name = "Italy" });
            },
            root => root
                .EfCoreSelect("read countries", (db, _) => db.Set<Country>())
                .Do("collect", c => collected.Add(c.Code)));

        Assert.False(status.Failed);
        Assert.Equal(new[] { "DE", "FR", "IT" }, collected.OrderBy(s => s));
        ctx.Dispose();
        cnx.Dispose();
    }

    // ===================================================================
    // EfCoreSelectSingle — one row per input value
    // ===================================================================

    [Fact]
    public async Task EfCoreSelectSingle_LoadsOnePerInput()
    {
        var collected = new ConcurrentBag<string>();

        var (status, ctx, cnx) = await RunAsync(
            "go",
            seed =>
            {
                seed.Countries.AddRange(
                    new Country { Code = "FR", Name = "France" },
                    new Country { Code = "DE", Name = "Germany" });
            },
            root => root
                .CrossApply("codes", _ => new[] { "FR", "DE" })
                .EfCoreSelectSingle(
                    "lookup country",
                    (db, code) => db.Set<Country>().Where(c => c.Code == code))
                .Do("collect", c => collected.Add(c.Name)));

        Assert.False(status.Failed);
        Assert.Equal(new[] { "France", "Germany" }, collected.OrderBy(s => s));
        ctx.Dispose();
        cnx.Dispose();
    }

    // ===================================================================
    // EfCoreSave — simple insert
    // ===================================================================

    [Fact]
    public async Task EfCoreSave_InsertsNewRows()
    {
        var (status, ctx, cnx) = await RunAsync<string>(
            "go",
            seed: null,
            root => root
                .CrossApply("rows", _ => new[]
                {
                    new Country { Code = "FR", Name = "France" },
                    new Country { Code = "BE", Name = "Belgium" },
                })
                .EfCoreSave("save countries", o => o.SeekOn(c => c.Code)));

        Assert.False(status.Failed);
        var saved = ctx.Countries.OrderBy(c => c.Code).Select(c => c.Code).ToList();
        Assert.Equal(new[] { "BE", "FR" }, saved);
        ctx.Dispose();
        cnx.Dispose();
    }

    // ===================================================================
    // EfCoreSave + SeekOn — upsert (insert or update on a business key)
    // ===================================================================

    [Fact]
    public async Task EfCoreSave_SeekOn_UpdatesExistingMatchingRow()
    {
        var (status, ctx, cnx) = await RunAsync<string>(
            "go",
            seed =>
            {
                seed.Countries.Add(new Country { Code = "FR", Name = "OLD" });
            },
            root => root
                .CrossApply("rows", _ => new[]
                {
                    new Country { Code = "FR", Name = "France" },     // existing → update
                    new Country { Code = "DE", Name = "Germany" },    // new      → insert
                })
                .EfCoreSave("upsert", o => o.SeekOn(c => c.Code)));

        Assert.False(status.Failed);
        var dict = ctx.Countries.ToDictionary(c => c.Code, c => c.Name);
        Assert.Equal("France", dict["FR"]);
        Assert.Equal("Germany", dict["DE"]);
        Assert.Equal(2, dict.Count);
        ctx.Dispose();
        cnx.Dispose();
    }

    // ===================================================================
    // EfCoreSave + DoNotUpdateIfExists — insert missing, leave existing
    // ===================================================================

    [Fact]
    public async Task EfCoreSave_DoNotUpdateIfExists_KeepsExistingValuesUnchanged()
    {
        var (status, ctx, cnx) = await RunAsync<string>(
            "go",
            seed => seed.Countries.Add(new Country { Code = "FR", Name = "OLD" }),
            root => root
                .CrossApply("rows", _ => new[]
                {
                    new Country { Code = "FR", Name = "WILL-NOT-OVERWRITE" },
                    new Country { Code = "ES", Name = "Spain" },
                })
                .EfCoreSave("save", o => o.SeekOn(c => c.Code).DoNotUpdateIfExists()));

        Assert.False(status.Failed);
        var dict = ctx.Countries.ToDictionary(c => c.Code, c => c.Name);
        Assert.Equal("OLD", dict["FR"]);
        Assert.Equal("Spain", dict["ES"]);
        ctx.Dispose();
        cnx.Dispose();
    }

    // ===================================================================
    // EfCoreLookup — enrich the stream with values from a DbSet
    // ===================================================================

    [Fact]
    public async Task EfCoreLookup_EnrichesStreamRowsWithEntityFromDb()
    {
        var collected = new ConcurrentBag<string>();

        var (status, ctx, cnx) = await RunAsync(
            "go",
            seed =>
            {
                seed.Countries.AddRange(
                    new Country { Code = "FR", Name = "France" },
                    new Country { Code = "DE", Name = "Germany" });
            },
            root => root
                .CrossApply("input", _ => new[]
                {
                    new { UserName = "alice", CountryCode = "FR" },
                    new { UserName = "bob",   CountryCode = "DE" },
                    new { UserName = "carol", CountryCode = "ZZ" }, // unmatched
                })
                .EfCoreLookup("with country", o => o
                    .Set<Country>()
                    .On(i => i.CountryCode, c => c.Code)
                    .Select((i, c) => $"{i.UserName}:{(c == null ? "?" : c.Name)}"))
                .Do("collect", collected.Add));

        Assert.False(status.Failed);
        Assert.Equal(
            new[] { "alice:France", "bob:Germany", "carol:?" },
            collected.OrderBy(s => s));
        ctx.Dispose();
        cnx.Dispose();
    }

    // ===================================================================
    // EfCoreLookup + CreateIfNotFound — auto-creates missing entities
    // ===================================================================

    [Fact]
    public async Task EfCoreLookup_CreateIfNotFound_InsertsMissingEntities()
    {
        var collected = new ConcurrentBag<string>();

        var (status, ctx, cnx) = await RunAsync(
            "go",
            seed => seed.Countries.Add(new Country { Code = "FR", Name = "France" }),
            root => root
                .CrossApply("input", _ => new[]
                {
                    new { UserName = "alice", CountryCode = "FR" },
                    new { UserName = "bob",   CountryCode = "JP" }, // not in DB → create
                })
                .EfCoreLookup("with country", o => o
                    .Set<Country>()
                    .On(i => i.CountryCode, c => c.Code)
                    .Select((i, c) => $"{i.UserName}:{c!.Name}")
                    .NoCacheFullDataset()
                    .CreateIfNotFound(i => new Country { Code = i.CountryCode, Name = i.CountryCode + "-auto" }))
                .Do("collect", collected.Add));

        Assert.False(status.Failed);
        Assert.Contains("alice:France", collected);
        Assert.Contains("bob:JP-auto", collected);
        ctx.Dispose();
        cnx.Dispose();
    }

    // ===================================================================
    // EfCoreDelete — remove rows matching a predicate
    // ===================================================================

    [Fact]
    public async Task EfCoreDelete_RemovesMatchingRows()
    {
        var (status, ctx, cnx) = await RunAsync<string>(
            "go",
            seed =>
            {
                seed.Countries.AddRange(
                    new Country { Code = "FR", Name = "France" },
                    new Country { Code = "DE", Name = "Germany" },
                    new Country { Code = "IT", Name = "Italy" });
            },
            root => root
                .CrossApply("codes to delete", _ => new[] { "DE", "IT" })
                .EfCoreDelete("delete countries", o => o
                    .Set<Country>()
                    .Where((code, c) => c.Code == code)));

        Assert.False(status.Failed);
        var remaining = ctx.Countries.Select(c => c.Code).ToList();
        Assert.Equal(new[] { "FR" }, remaining);
        ctx.Dispose();
        cnx.Dispose();
    }
}
