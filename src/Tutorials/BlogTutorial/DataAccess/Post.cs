using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogTutorial.DataAccess
{
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
    public class PostConfiguration : IEntityTypeConfiguration<Post>
    {
        public void Configure(EntityTypeBuilder<Post> builder)
        {
            builder.ToTable(nameof(Post));
            builder.HasKey(i => i.Id);
            builder.HasIndex(i => new { i.AuthorId, i.DateTime }).IsUnique();
            builder.Property(i => i.Id).UseIdentityColumn();
            builder.HasOne(i => i.Author).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.AuthorId);
            builder.HasOne(i => i.Category).WithMany().OnDelete(DeleteBehavior.Restrict).HasForeignKey(i => i.CategoryId);
        }
    }
}