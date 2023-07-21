namespace Paillave.Etl.EntityFrameworkCore.Tests
{
    public class SqlDatabaseTests : IClassFixture<MsSqlDatabaseFixture>, IAsyncLifetime
    {
        protected readonly BloggingContext Context;

        public SqlDatabaseTests(MsSqlDatabaseFixture fixture)
        {
            Context = new BloggingContext(fixture.GenerateDatabaseConnectionString());
        }

        public Task InitializeAsync() => Context.Database.EnsureCreatedAsync();

        public async Task DisposeAsync()
        {
            await Context.Database.EnsureDeletedAsync();
            await Context.DisposeAsync();
        }
    }
}
