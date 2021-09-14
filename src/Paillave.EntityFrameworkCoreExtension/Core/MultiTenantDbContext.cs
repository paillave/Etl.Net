using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Paillave.EntityFrameworkCoreExtension.MigrationOperations;

namespace Paillave.EntityFrameworkCoreExtension.Core
{
    public class MultiTenantDbContext : DbContext
    {
        private object _sync = new object();
        private readonly string collation;

        public int TenantId { get; }

        // https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations
        // https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli

        public MultiTenantDbContext(DbContextOptions options, int tenantId, string collation = null) : base(options)
        {
            this.TenantId = tenantId;
            this.collation = collation;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // For EFCore5
            if (!string.IsNullOrWhiteSpace(collation))
            {
                // modelBuilder.UseCollation(collation);
            }
            // https://github.com/aspnet/EntityFrameworkCore/blob/a9c6cb3548df771a57af97f0aafe55009464f8f9/src/EFCore/ModelBuilder.cs#L266
            ApplyConfigurationsFromAssembly(modelBuilder);
        }
        private void ApplyConfigurationsFromAssembly(ModelBuilder modelBuilder)
        {
            var applyEntityConfigurationMethod = typeof(ModelBuilder)
                .GetMethods()
                .Single(e =>
                    e.Name == nameof(ModelBuilder.ApplyConfiguration)
                    && e.ContainsGenericParameters
                    && e.GetParameters().SingleOrDefault()?.ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>));

            foreach (var type in this.GetType().Assembly.GetConstructibleTypes())
            {
                foreach (var @interface in type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)))
                {
                    var target = applyEntityConfigurationMethod.MakeGenericMethod(@interface.GenericTypeArguments[0]);
                    if (type.GetConstructor(Type.EmptyTypes) != null)
                    {
                        target.Invoke(modelBuilder, new[] { Activator.CreateInstance(type) });
                    }
                    else if (type.GetConstructor(new Type[] { this.GetType() }) != null)
                    {
                        target.Invoke(modelBuilder, new[] { Activator.CreateInstance(type, new object[] { this }) });
                    }
                }
            }
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>();
            base.OnConfiguring(optionsBuilder);
        }
        public override int SaveChanges()
        {
            lock (_sync)
            {
                ChangeTracker.DetectChanges();
                foreach (var item in ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(i => i.Entity).ToList())
                {
                    UpdateEntityForMultiTenancy(item);
                }
                return base.SaveChanges();
            }
        }
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                ChangeTracker.DetectChanges();
                foreach (var item in ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(i => i.Entity).ToList())
                {
                    UpdateEntityForMultiTenancy(item);
                }
                return base.SaveChangesAsync(cancellationToken);
            }
        }
        public void UpdateEntitiesForMultiTenancy<T>(IEnumerable<T> items)
        {
            foreach (var item in items) this.UpdateEntityForMultiTenancy(item);
        }
        public T UpdateEntityForMultiTenancy<T>(T item)
        {
            if (TenantId != 0 && item is IMultiTenantEntity multiTenantEntity)
            {
                multiTenantEntity.TenantId = TenantId;
            }
            return item;
        }
    }
}
