using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlogTutorial.DataAccess
{
    public class LinkPost : Post
    {
        public Uri Url { get; set; }
    }
    public class LinkPostConfiguration : IEntityTypeConfiguration<LinkPost>
    {
        public void Configure(EntityTypeBuilder<LinkPost> builder)
        {
            builder.HasBaseType<Post>();
            builder.Property(i => i.Url).IsRequired().HasConversion(new UriValueConverter());
        }
    }
    public class UriValueConverter : ValueConverter<Uri, string>
    {
        public UriValueConverter() : base(i => Serialize(i), i => Deserialize(i)) { }
        private static string Serialize(Uri uri) => uri == null ? null : uri.ToString();
        private static Uri Deserialize(string value) => string.IsNullOrWhiteSpace(value) ? null : new Uri(value);
    }
}