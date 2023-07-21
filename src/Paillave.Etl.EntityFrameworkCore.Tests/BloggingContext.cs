namespace Paillave.Etl.EntityFrameworkCore.Tests
{
    public class Blog
    {
        public int BlogId { get; set; }
        public string Name { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Name { get; set; }
    }

    public class BloggingContext : DbContext
    {
        private readonly string _connectionString;

        public BloggingContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(e => e.BlogId);
                entity.Property(e => e.Name).HasMaxLength(20);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.HasKey(e => e.PostId);
                entity.Property(e => e.PostId).ValueGeneratedNever();
                entity.Property(e => e.Name).HasMaxLength(20);
            });
        }
    }
}
