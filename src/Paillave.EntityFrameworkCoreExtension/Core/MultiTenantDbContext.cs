using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Paillave.EntityFrameworkCoreExtension.MigrationOperations;
using System.Collections;
using System.Reflection;

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
    public void UpdateEntitiesForMultiTenancy<T>(IEnumerable<T> items) where T : class
    {
        foreach (var item in items) this.UpdateEntityForMultiTenancy(item);
    }
    public void UpdateEntityForMultiTenancy(object item)
    {
        var processed = new HashSet<object>();
        InnerUpdateEntityForMultiTenancy(processed, item);
    }
    private readonly Dictionary<Type, PropertyInfo[]> _propertyInfosToSetForMultitenancies = new Dictionary<Type, PropertyInfo[]>();
    private readonly object _propertyInfosToSetForMultitenanciesLock = new object();
    private PropertyInfo[] GetPropertiesForMultiTenancy(Type type)
    {
        lock (_propertyInfosToSetForMultitenanciesLock)
        {
            if (_propertyInfosToSetForMultitenancies.TryGetValue(type, out var properties)) return properties;
            properties = type.GetProperties().Where(i => (i.PropertyType.IsClass && i.PropertyType.IsAssignableTo(typeof(IMultiTenantEntity))) || i.PropertyType.IsAssignableTo(typeof(IEnumerable))).ToArray();
            _propertyInfosToSetForMultitenancies[type] = properties;
            return properties;
        }
    }
    private void InnerUpdateEntityForMultiTenancy(HashSet<object> hashset, object item)
    {
        if (hashset.Contains(item)) return;
        hashset.Add(item);
        if (item is IEnumerable array)
        {
            foreach (var elt in array)
            {
                InnerUpdateEntityForMultiTenancy(hashset, elt);
            }
        }
        else
        if (TenantId != 0 && item is IMultiTenantEntity multiTenantEntity)
        {
            multiTenantEntity.TenantId = TenantId;
            var propertyValues = GetPropertiesForMultiTenancy(item.GetType()).Select(p => p.GetValue(item)).Where(i => i != null).ToList();
            foreach (var pv in propertyValues)
                InnerUpdateEntityForMultiTenancy(hashset, pv!);
        }
    }
}
