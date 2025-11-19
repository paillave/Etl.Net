using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Paillave.EntityFrameworkCoreExtension.Core;

public interface ITenantProvider
{
    int Current { get; }
}
public class MultiTenantDbContext : DbContext
{
    private readonly ITenantProvider tenantProvider;
    private readonly object _sync = new();

    public MultiTenantDbContext(DbContextOptions options) : base(options)
    {
        tenantProvider = this.GetService<ITenantProvider>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        SetupMultiTenant(modelBuilder);
    }
    protected void SetupMultiTenant(ModelBuilder modelBuilder)
    {
        var genericMethod = this.GetType()
            .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            .Single(m => m.Name == nameof(SetupTypedMultiTenant) && m.IsGenericMethod && m.GetParameters().Length == 1);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(et => et.FindProperty("TenantId") != null))
            genericMethod
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, [modelBuilder]);
    }

    protected void SetupTypedMultiTenant<TEntity>(ModelBuilder modelBuilder) where TEntity : class
    {
        var entityTypeBuilder = modelBuilder.Entity<TEntity>();
        var entityType = entityTypeBuilder.Metadata;
        if (entityType.FindProperty("TenantId") != null && entityType.GetQueryFilter() == null && entityType.BaseType == null)
            //     entityTypeBuilder.Property<int>("TenantId").IsRequired();
            // if (entityType.GetQueryFilter() == null)
            entityTypeBuilder.HasQueryFilter(i => EF.Property<int>(i, "TenantId") == tenantProvider.Current);
    }
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.EnableSensitiveDataLogging(true);
    //     optionsBuilder.ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>();
    //     base.OnConfiguring(optionsBuilder);
    // }
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
        var tenantId = tenantProvider.Current;
        entityEntry.Property("TenantId").CurrentValue = tenantId;
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
