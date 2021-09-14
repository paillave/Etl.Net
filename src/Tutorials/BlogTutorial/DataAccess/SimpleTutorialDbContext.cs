using System;
using Microsoft.EntityFrameworkCore;

namespace BlogTutorial.DataAccess
{
    public class SimpleTutorialDbContext : DbContext
    {
        private readonly string _connectionString = null;
        public SimpleTutorialDbContext() { }
        public SimpleTutorialDbContext(string connectionString) => _connectionString = connectionString;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString ?? @"Server=localhost,1433;Database=BlogTutorial;user=BlogTutorial;password=TestEtl.TestEtl;MultipleActiveResultSets=True");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var authorBuilder = modelBuilder.Entity<Author>();
            authorBuilder.ToTable(nameof(Author));
            authorBuilder.HasKey(i => i.Id);
            authorBuilder.HasIndex(i => i.Email).IsUnique();
            authorBuilder.Property(i => i.Id).UseIdentityColumn();
            authorBuilder.Property(i => i.Name).IsRequired();
            authorBuilder.Property(i => i.Email).HasMaxLength(250).IsRequired();

            var categoryBuilder = modelBuilder.Entity<Category>();
            categoryBuilder.ToTable(nameof(Category));
            categoryBuilder.HasKey(i => i.Id);
            categoryBuilder.HasIndex(i => i.Code).IsUnique();
            categoryBuilder.Property(i => i.Id).UseIdentityColumn();
            categoryBuilder.Property(i => i.Name).IsRequired();
            categoryBuilder.Property(i => i.Code).IsRequired().HasMaxLength(20);

            var postBuilder = modelBuilder.Entity<Post>();
            postBuilder.ToTable(nameof(Post));
            postBuilder.HasKey(i => i.Id);
            postBuilder.HasIndex(i => new { i.AuthorId, i.DateTime }).IsUnique();
            postBuilder.Property(i => i.Id).UseIdentityColumn();
            postBuilder.HasOne(i => i.Author).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.AuthorId);
            postBuilder.HasOne(i => i.Category).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.CategoryId);

            var linkPostBuilder = modelBuilder.Entity<LinkPost>();
            linkPostBuilder.HasBaseType<Post>();
            linkPostBuilder.Property(i => i.Url).IsRequired().HasConversion(
                uri => uri == null ? null : uri.ToString(),
                value => string.IsNullOrWhiteSpace(value) ? null : new Uri(value));

            var textPostBuilder = modelBuilder.Entity<TextPost>();
            textPostBuilder.HasBaseType<Post>();
            textPostBuilder.Property(i => i.Text).IsRequired();

            var executionLogBuilder = modelBuilder.Entity<ExecutionLog>();
            executionLogBuilder.ToTable(nameof(ExecutionLog));
            executionLogBuilder.HasKey(i => i.Id);
            executionLogBuilder.Property(i => i.Id).UseIdentityColumn();
            executionLogBuilder.Property(i => i.EventType).HasMaxLength(250).IsRequired();
            executionLogBuilder.Property(i => i.Message).IsRequired();
        }
    }
    public class Author
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
    public class Category
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
    public abstract class Post
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Title { get; set; }
        public int AuthorId { get; set; }
        public Author Author { get; set; }
        public int? CategoryId { get; set; }
        public Category Category { get; set; }
    }
    public class LinkPost : Post
    {
        public Uri Url { get; set; }
    }
    public class TextPost : Post
    {
        public string Text { get; set; }
    }
    public class ExecutionLog
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public Guid ExecutionId { get; set; }
        public string EventType { get; set; }
        public string Message { get; set; }
    }
}