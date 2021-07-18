using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlogTutorial.DataAccess
{
    public class TextPost : Post
    {
        public string Text { get; set; }
    }
    public class TextPostConfiguration : IEntityTypeConfiguration<TextPost>
    {
        public void Configure(EntityTypeBuilder<TextPost> builder)
        {
            builder.HasBaseType<Post>();
            builder.Property(i => i.Text).IsRequired();
        }
    }
}