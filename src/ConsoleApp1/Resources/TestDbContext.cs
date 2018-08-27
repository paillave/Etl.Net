using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1.Resources
{
    public class TestDbContext : DbContext
    {
        public DbSet<MyInputValue> Input { get; set; }
        public DbSet<MyOutputValue> Output { get; set; }
        public TestDbContext(string connectionString) : base(new DbContextOptionsBuilder<TestDbContext>().UseSqlServer(connectionString).Options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MyInputValueConfiguration());
            modelBuilder.ApplyConfiguration(new MyOutputValueConfiguration());
        }
        class MyInputValueConfiguration : IEntityTypeConfiguration<MyInputValue>
        {
            public void Configure(EntityTypeBuilder<MyInputValue> builder)
            {
                builder.ToTable("InputValue");
                builder.HasKey(i => i.Id);
                builder.Property(i => i.Id).UseSqlServerIdentityColumn();
                builder.Property(i => i.Name).HasMaxLength(50);
            }
        }
        class MyOutputValueConfiguration : IEntityTypeConfiguration<MyOutputValue>
        {
            public void Configure(EntityTypeBuilder<MyOutputValue> builder)
            {
                builder.ToTable("OutputValue");
                builder.HasKey(i => i.Id);
                builder.Property(i => i.Id).UseSqlServerIdentityColumn();
                builder.Property(i => i.Name).HasMaxLength(50);
            }
        }
    }
    public class MyInputValue
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class MyOutputValue
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
