using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;

namespace Paillave.Etl.EntityFrameworkCore.Tests
{
    public class MsSqlDatabaseFixture : IAsyncLifetime
    {
        private readonly MsSqlContainer _dbContainer;

        public MsSqlDatabaseFixture()
        {
            _dbContainer = new MsSqlBuilder().Build();
        }

        public Task InitializeAsync() => _dbContainer.StartAsync();


        public Task DisposeAsync() => _dbContainer.DisposeAsync().AsTask();

        public string GenerateDatabaseConnectionString()
        {
            var builder = new SqlConnectionStringBuilder(_dbContainer.GetConnectionString());
            builder.InitialCatalog = $"Test_{Guid.NewGuid().ToString("N")}";
            return builder.ConnectionString;
        }
    }
}
