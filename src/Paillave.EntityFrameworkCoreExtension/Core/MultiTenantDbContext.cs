using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Paillave.EntityFrameworkCoreExtension.MigrationOperations;
using System.Collections;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Paillave.EntityFrameworkCoreExtension.Core;

public class MultiTenantDbContext : DbContext
{
    private object _sync = new object();
    private readonly string? _collation;

    public MultiTenantDbContext(int tenantId)
    {
        this.TenantId = tenantId;

    }
    public int TenantId { get; }
    private Dictionary<Type, IEntityType>? _inTenantEntities = null;
    // private record EntitySummary(IProperty TenantIdProperty)
    private Dictionary<Type, IEntityType> InTenantEntities => _inTenantEntities ??= this.Model.GetEntityTypes().Where(i => i.FindProperty("TenantId") != null).ToDictionary(i => i.ClrType);

    // https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations
    // https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli

    public MultiTenantDbContext(DbContextOptions options, int tenantId, string? collation = null) : base(options)
    {
        this.TenantId = tenantId;
        this._collation = collation;
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // For EFCore5
        if (!string.IsNullOrWhiteSpace(_collation))
        {
            modelBuilder.UseCollation(_collation);
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
                    target.Invoke(modelBuilder, [Activator.CreateInstance(type, [this])]);
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
            var processed = new HashSet<object>();
            foreach (var item in ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList())
            {
                InnerUpdateEntityForMultiTenancy(processed, item);
            }
            OnBeforeSaveChanges();
            return base.SaveChanges();
        }
    }
    public virtual void OnBeforeSaveChanges()
    {
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        lock (_sync)
        {
            ChangeTracker.DetectChanges();
            var processed = new HashSet<object>();
            foreach (var item in ChangeTracker.Entries().Where(e => e.State == EntityState.Added).ToList())
            {
                InnerUpdateEntityForMultiTenancy(processed, item);
            }
            OnBeforeSaveChanges();
            return base.SaveChangesAsync(cancellationToken);
        }
    }
    public void UpdateEntitiesForMultiTenancy<T>(IEnumerable<T> items) where T : class
    {
        var processed = new HashSet<object>();
        if (items == null) return;
        foreach (var item in items) this.InnerUpdateEntityForMultiTenancy(processed, Entry(item));
    }
    public void UpdateEntityForMultiTenancy(object item)
    {
        var processed = new HashSet<object>();
        if (item == null) return;
        InnerUpdateEntityForMultiTenancy(processed, Entry(item));
    }
    private void InnerUpdateEntityForMultiTenancy(HashSet<object> processedEntities, EntityEntry entityEntry)
    {
        if (processedEntities.Contains(entityEntry.Entity)) return;
        processedEntities.Add(entityEntry.Entity);
        if (TenantId == 0 || !InTenantEntities.TryGetValue(entityEntry.Metadata.ClrType, out var entityType)) return;
        entityEntry.Property("TenantId").CurrentValue = TenantId;
        foreach (var navigation in entityEntry.Navigations)
        {
            if (navigation.CurrentValue != null)
            {
                if (navigation.Metadata.IsCollection)
                {
                    foreach (var subItem in (navigation.CurrentValue as IEnumerable)?.Cast<object>() ?? [])
                        InnerUpdateEntityForMultiTenancy(processedEntities, Entry(subItem));
                }
                else
                {
                    InnerUpdateEntityForMultiTenancy(processedEntities, Entry(navigation.CurrentValue));
                }
            }
        }
    }
}
