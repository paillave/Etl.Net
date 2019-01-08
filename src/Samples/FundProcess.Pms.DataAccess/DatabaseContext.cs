using System;
using Microsoft.EntityFrameworkCore;
using FundProcess.Pms.DataAccess.Configurations;
using System.Linq;
using FundProcess.Pms.DataAccess.Schemas.Entity.Tables;
using FundProcess.Pms.DataAccess.Schemas.UserAccounts.Tables;

namespace FundProcess.Pms.DataAccess
{
    public class DatabaseContext : DbContext
    {
        private object _sync = new object();
        private TenantContext _tenantContext;
        public DatabaseContext(DbContextOptions<DatabaseContext> options, TenantContext tenantContext) : base(options)
        {
            this._tenantContext = tenantContext;
        }
        // public DbSet<Company> Companies { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserLogin> Logins { get; set; }
        public DbSet<UserEntityRole> Rights { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.Model.SetChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
            // base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly, _tenantContext);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging(true);
            base.OnConfiguring(optionsBuilder);
        }
        public override int SaveChanges()
        {
            lock (_sync)
            {
                ChangeTracker.DetectChanges();
                foreach (var item in ChangeTracker.Entries().Where(e => e.State == EntityState.Added).Select(i => i.Entity).ToList())
                {
                    UpdateForMultiTenancy(item);
                }
                return base.SaveChanges();
            }
        }

        //public void UpdateForMultiTenancy(object item)
        //{
        //    IBelongsToEntity entity = item as IBelongsToEntity;
        //    if (entity != null && entity.BelongsToEntityId == null && _tenantContext.EntityId != 0)
        //        entity.BelongsToEntityId = _tenantContext.EntityId;
        //    IBelongsToEntityGroup entityGroup = item as IBelongsToEntityGroup;
        //    if (entityGroup != null && entityGroup.BelongsToEntityGroupId == null && _tenantContext.EntityGroupId != 0)
        //        entityGroup.BelongsToEntityGroupId = _tenantContext.EntityGroupId;
        //}
        public T UpdateForMultiTenancy<T>(T item)
        {
            IBelongsToEntity entity = item as IBelongsToEntity;
            if (entity != null && entity.BelongsToEntityId == null && _tenantContext.EntityId != 0)
                entity.BelongsToEntityId = _tenantContext.EntityId;
            IBelongsToEntityGroup entityGroup = item as IBelongsToEntityGroup;
            if (entityGroup != null && entityGroup.BelongsToEntityGroupId == null && _tenantContext.EntityGroupId != 0)
                entityGroup.BelongsToEntityGroupId = _tenantContext.EntityGroupId;
            return item;
        }
    }
}
