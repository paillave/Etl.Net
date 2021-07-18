using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogTutorial.DataAccess
{
    public class Author
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
    }
    public class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.ToTable(nameof(Author));
            builder.HasKey(i => i.Id);
            builder.HasIndex(i => i.Email).IsUnique();
            builder.Property(i => i.Id).UseIdentityColumn();
            builder.Property(i => i.Name).IsRequired();
            builder.Property(i => i.Email).HasMaxLength(250).IsRequired();
        }
    }
}